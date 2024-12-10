using System.Collections;
using System.Linq;
using UnityEngine;

public abstract class NPCCharacterController : BaseCharacterController
{
    protected enum CurrentEnemyState
    {
        Idle,
        SawEnemy,
        Combat
    }
    [SerializeField] protected CurrentEnemyState currentState;

    Vector2 LookDirection;
    public float FOV { get; protected set; } = 60f;

    [SerializeField] protected float TimerUntilStopsSeeing = 0f;
    [SerializeField] protected float EnemySawForgetTime = 20f;
    [SerializeField] protected float EnemySeeRadius = 20f;

    [SerializeField] protected LayerMask GroundLayer;

    [SerializeField] protected Vector2 MovePosition;
    [SerializeField] protected Vector2 MoveDirection;
    [SerializeField] protected float DistenceToMovePosition;

    [SerializeField] protected float angleToEnemy;
    [SerializeField] protected Vector2 DirectionToEnemy;
    [SerializeField] protected GameObject EnemyFocused;
    [SerializeField] protected float DistenceToEnemy;

    [SerializeField] protected Coroutine CurrentBehaviourControllerCoroutine;
    protected void Start()
    {
        base.Start();
        airVelocity = 100f;
        StateManager(CurrentEnemyState.Idle);
        InvokeRepeating("CheckForEnemies", 0, 0.2f);
    }

    //-------------------collision stuff----------------------------------
    void OnCollisionStay2D(Collision2D collision)
    {
        OnGroundCheck(collision);

        if (Grounded == true)
        {
            SomethingInTheWayCheck(collision);
        }
    }

    protected void SomethingInTheWayCheck(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            float RoundedMoveDirectionX = (MoveDirection.x > 0) ? Mathf.Ceil(MoveDirection.x) : Mathf.Floor(MoveDirection.x);
            if (contact.normal.x == -RoundedMoveDirectionX && contact.normal.x != 0 && !IsOutOfJumps)
            {
                Jump();
                break;
            }
        }
    }

    //-------------Checking for Enemies in different states------------------

    protected void CheckForEnemies()
    {
        switch (currentState)
        {
            case CurrentEnemyState.Idle:
                CheckForEnemiesIdle();
                break;
            case CurrentEnemyState.SawEnemy:
                CheckForEnemyAllerted();
                break;
            case CurrentEnemyState.Combat:
                CheckForEnemyCombat();
                break;
        }
    }

    protected void CheckForEnemiesIdle()
    {
        Collider2D[] EnemiesInArea = Physics2D.OverlapCircleAll(transform.position, EnemySeeRadius, EnemyLayer);

        foreach (Collider2D Enemy in EnemiesInArea)
        {
            Vector2 DirectionToEnemy = (Enemy.gameObject.transform.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, DirectionToEnemy, EnemySeeRadius, ~RaycastIgnoreLayer);
            float AngleToEnemy = Vector2.Angle(HeadPivot.transform.right * (transform.localScale.x / 3), DirectionToEnemy);
            Debug.Log(AngleToEnemy);
            if(hit) Debug.Log(hit.collider.gameObject);
            Debug.Log(Enemy.gameObject);

            if (Mathf.Abs(AngleToEnemy) <= FOV && hit && hit.collider.gameObject == Enemy.gameObject)
            {
                Debug.Log("Saw in FOV!");
                EnemyFocused = Enemy.gameObject;
                StateManager(CurrentEnemyState.SawEnemy);
            }
            else if (Vector2.Distance(transform.position, Enemy.gameObject.transform.position) < 3f)
            {
                EnemyFocused = Enemy.gameObject;
                StateManager(CurrentEnemyState.SawEnemy);
            }
        }
    }

    protected void CheckForEnemyAllerted()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, DirectionToEnemy, EnemySeeRadius, EnemyLayer & ~RaycastIgnoreLayer);
        DistenceToEnemy = Vector2.Distance(transform.position, EnemyFocused.transform.position);
        if (TimerUntilStopsSeeing <= 0)
        {
            StateManager(CurrentEnemyState.Idle);
        }


        if (hit.collider == null || hit.collider.gameObject != EnemyFocused)
        {
            TimerUntilStopsSeeing -= 0.1f;
        }
        else if (DistenceToEnemy < 3f)
        {
            StateManager(CurrentEnemyState.Combat);
        }
        else
        {
            TimerUntilStopsSeeing = EnemySawForgetTime;
        }
    }

    protected abstract void CheckForEnemyCombat();

    //------------------current state controller----------------------

    protected void StateManager(CurrentEnemyState state)
    {
        Debug.Log($"StateManager called with state: {state}");
        switch (state)
        {
            case CurrentEnemyState.Idle:
                ChangeCurrentStateToIdle();
                break;
            case CurrentEnemyState.SawEnemy:
                ChangeCurrentStateToSawEnemy();
                break;
            case CurrentEnemyState.Combat:
                ChangeCurrentStateToCombat();
                break;
        }
    }

    protected void ChangeCurrentStateToIdle()
    {
        if (Running) Running = false;
        if (EnemyFocused != null) EnemyFocused = null;

        TimerUntilStopsSeeing = 0f;
        currentState = CurrentEnemyState.Idle;

        if (CurrentBehaviourControllerCoroutine != null)
        {
            StopCoroutine(CurrentBehaviourControllerCoroutine);
        }
        CurrentBehaviourControllerCoroutine = StartCoroutine(BehaviourControllerIdle());
    }

    protected void ChangeCurrentStateToSawEnemy()
    {
        currentState = CurrentEnemyState.SawEnemy;
        if (CurrentBehaviourControllerCoroutine != null)
        {
            StopCoroutine(CurrentBehaviourControllerCoroutine);
        }
        CurrentBehaviourControllerCoroutine = StartCoroutine(BehaviourControllerSawEnemy());
        TimerUntilStopsSeeing = EnemySawForgetTime;
    }

    protected void ChangeCurrentStateToCombat()
    {
        if (Running) Running = false;
        currentState = CurrentEnemyState.Combat;
        if (CurrentBehaviourControllerCoroutine != null)
        {
            StopCoroutine(CurrentBehaviourControllerCoroutine);
        }
        CurrentBehaviourControllerCoroutine = StartCoroutine(BehaviourControllerCombat());
        TimerUntilStopsSeeing = 0f;
    }

    //----------------current behaviour controller---------------------------

    protected virtual IEnumerator BehaviourControllerIdle()
    {
        while (currentState == CurrentEnemyState.Idle)
        {
            ChangeMovePositionIdle();
            yield return new WaitForSeconds(8f);
        }
    }

    protected void ChangeMovePositionIdle()
    {
        Vector2 randomPoint2D = new Vector2(transform.position.x, transform.position.y) + UnityEngine.Random.insideUnitCircle * 12f;
        RaycastHit2D hit;
        if (Physics2D.OverlapPoint(randomPoint2D, GroundLayer))
        {
            ChangeMovePositionIdle();
            return;
        }
        hit = Physics2D.Raycast(randomPoint2D, Vector2.down, 50f, GroundLayer);

        MovePosition = new Vector2(hit.point.x, hit.point.y + 1.5f);
    }

    protected abstract IEnumerator BehaviourControllerSawEnemy();
    protected abstract void EnemySawBehaviour();
    protected abstract void ChangeMovePositionEnemySaw();
    protected abstract IEnumerator BehaviourControllerCombat();
    protected abstract void CombatBehaviour(ref float AttackCoolDownTimer);
    protected abstract void ChangeMovePositionCombat();
    protected abstract void ActionPattern(ref float AttackCoolDownTimer);

    //---------------------Movement--------------------------

    protected void MoveToTheNextPoint()
    {
        CheckIfAtTheDestination();

        float Speed = GetSpeedValue();

        Move(Speed);
    }

    protected void CheckIfAtTheDestination()
    {
        MoveDirection = new Vector2(MovePosition.x - transform.position.x, MovePosition.y - transform.position.y).normalized;
        DistenceToMovePosition = Vector2.Distance(transform.position, MovePosition);
        if ((Moving || Running) && DistenceToMovePosition < 1f)
        {
            Moving = false;
            Running = false;
        }
        else if (Moving == false && DistenceToMovePosition > 2f)
        {
            Moving = true;
        }
    }

    protected float GetSpeedValue()
    {
        if (Running && characterStatController.ReduceStamina(Time.fixedDeltaTime * 10f))
        {
            return baseSpeed * runSpeedModifier;
        }
        else if (Moving)
        {
            return baseSpeed;
        }
        else
        {
            return 0;
        }
    }

    protected void Move(float SpeedValue)
    {
        if (Grounded)
        {
            rb.linearVelocity = new Vector2(MoveDirection.x * SpeedValue, rb.linearVelocity.y);
        }
        else
        {
            rb.AddForce(new Vector2(MoveDirection.x * airVelocity * SpeedValue, 0), ForceMode2D.Force);
        }
    }

    //-----------------Sprite Rotation---------------------------

    protected void HandleRotate()
    {
        UpdateAngleToTarget();

        Vector2 CurrentDirectionTarget = (currentState == CurrentEnemyState.Idle) ? MoveDirection : DirectionToEnemy;

        if (CurrentDirectionTarget.x < 0)
        {
            RotateCharacterSpritesLeft();
        }
        else
        {
            RotateCharacterSpritesRight();
        }
    }

    protected void UpdateAngleToTarget()
    {
        if (currentState != CurrentEnemyState.Idle)
        {
            DirectionToEnemy = new Vector2(EnemyFocused.transform.position.x - transform.position.x, EnemyFocused.transform.position.y - transform.position.y).normalized;
            angleToEnemy = Mathf.Atan2(DirectionToEnemy.y, DirectionToEnemy.x) * Mathf.Rad2Deg;
        }
    }

    protected void RotateCharacterSpritesLeft()
    {
        transform.localScale = new Vector3(-3, transform.localScale.y, transform.localScale.z);

        if (currentState != CurrentEnemyState.Idle)
        {
            float angletoTargetCalculation = -(180 - angleToEnemy);
            if (angletoTargetCalculation < -90)
            {
                angletoTargetCalculation = angletoTargetCalculation + 360;//dont ask me why this works(it does) 
            }

            TorsoPivot.transform.rotation = Quaternion.Euler(new Vector3(TorsoPivot.transform.rotation.x, TorsoPivot.transform.rotation.y, angletoTargetCalculation / (180 / maxTorsoRotation)));
            HeadPivot.transform.rotation = Quaternion.Euler(new Vector3(HeadPivot.transform.rotation.x, HeadPivot.transform.rotation.y, angletoTargetCalculation / (180 / (maxHeadRotation + maxTorsoRotation))));
            ArmRightPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmRightPivot.transform.rotation.x, ArmRightPivot.transform.rotation.y, angletoTargetCalculation / (180 / (maxHandRotation + maxTorsoRotation))));
            ArmLeftPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmLeftPivot.transform.rotation.x, ArmLeftPivot.transform.rotation.y, angletoTargetCalculation / (180 / (maxHandRotation + maxTorsoRotation))));
        }
    }

    protected void RotateCharacterSpritesRight()
    {
        transform.localScale = new Vector3(3, transform.localScale.y, transform.localScale.z);
        if (currentState != CurrentEnemyState.Idle)
        {
            TorsoPivot.transform.rotation = Quaternion.Euler(new Vector3(TorsoPivot.transform.rotation.x, TorsoPivot.transform.rotation.y, angleToEnemy / (180 / maxTorsoRotation)));
            HeadPivot.transform.rotation = Quaternion.Euler(new Vector3(HeadPivot.transform.rotation.x, HeadPivot.transform.rotation.y, angleToEnemy / (180 / (maxHeadRotation + maxTorsoRotation))));
            ArmRightPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmRightPivot.transform.rotation.x, ArmRightPivot.transform.rotation.y, angleToEnemy / (180 / (maxHandRotation + maxTorsoRotation))));
            ArmLeftPivot.transform.rotation = Quaternion.Euler(new Vector3(ArmLeftPivot.transform.rotation.x, ArmLeftPivot.transform.rotation.y, angleToEnemy / (180 / (maxHandRotation + maxTorsoRotation))));
        }
    }
}
