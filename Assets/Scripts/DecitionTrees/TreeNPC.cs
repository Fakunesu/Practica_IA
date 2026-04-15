using System.Runtime.CompilerServices;
using UnityEngine;

public class TreeNPC : MonoBehaviour
{
    public Transform target;
    public Transform safePlace;
    public Transform storage;

    [SerializeField] private bool isNight;
    [SerializeField] private float timerToChange = 5f;
    private float ciclingTimer;

    [SerializeField] private float currentHP = 5f;
    private int maxHP = 10;
    private float speed = 3f;

    private bool isFullRested;

    [SerializeField] private int woodAmount;
    private int maxWoodAmount = 3;

    private ITreeeNode root;

    void Start()
    {
        ActionNode heal = new ActionNode(Heal);
        ActionNode collectWood = new ActionNode(CollectWood);
        ActionNode goToHouse = new ActionNode(GoToHouse);
        ActionNode goToWood = new ActionNode(GoToWood);
        ActionNode goToStorage = new ActionNode(GoToStorage);
        ActionNode depositWood = new ActionNode(DepositWood);

        QuestionNode closeToHouse = new QuestionNode(CloseToHouse, heal, goToHouse);
        QuestionNode isInRangeOfWood = new QuestionNode(IsInRangeOfWood, collectWood, goToWood);
        QuestionNode isInRangeOfStorage = new QuestionNode(IsInRangeOfStorage, depositWood, goToStorage);
        QuestionNode hasCapacity = new QuestionNode(HasCapacity, isInRangeOfWood, isInRangeOfStorage);
        QuestionNode isNightNode = new QuestionNode(IsNight, closeToHouse, hasCapacity);
        QuestionNode isInjured = new QuestionNode(IsInjured, closeToHouse, isNightNode);

        root = isInjured;
    }

    void Update()
    {

        CicleOfDay();

        if (root != null)
        {
            root.Execute();
        }
    }

    private bool IsNight() => isNight;

    private bool IsFullyRested() => isFullRested;

    private bool CloseToHouse()
    {
        float dist = Vector3.Distance(transform.position, safePlace.position);
        return dist < 0.2f;
    }

    private bool HasCapacity()
    {
        if (woodAmount < maxWoodAmount)
        {
            return true;
        }
        else { return false; }
    }

    private bool IsInjured()
    {
        return currentHP < maxHP;
    }

    private bool IsInRangeOfWood()
    {
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);
        return dist < 2f;
    }

    private bool IsInRangeOfStorage()
    {
        if (storage == null) return false;

        float dist = Vector3.Distance(transform.position, storage.position);
        return dist < 2f;
    }

    private void CollectWood()
    {
        if (target == null) return;

        woodAmount++;
        if (woodAmount >= maxWoodAmount)
        {
            HasCapacity();
        }
        Debug.Log("Collecting wood");
    }

    private void DepositWood()
    {
        woodAmount = 0;
        Debug.Log("Depositing wood");
    }

    private void Heal()
    {
        if (currentHP < maxHP)
        {
            currentHP += Time.deltaTime;
            if (currentHP > maxHP)
                currentHP = maxHP;

            Debug.Log("Healing");
        }
    }

    private void GoToWood()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
        Debug.Log("Going to wood");
    }

    private void GoToHouse()
    {
        if (safePlace == null) return;

        Vector3 dir = safePlace.position - transform.position;
        transform.position += dir.normalized * speed * Time.deltaTime;
        Debug.Log("Going to house");
    }

    private void GoToStorage()
    {
        if (storage == null)
        {
            Debug.Log("NoStorage");
            return;
        }
        else
        {
            Vector3 dir = storage.position - transform.position;
            transform.position += dir.normalized * speed * Time.deltaTime;
            Debug.Log("Going to storage");
        }
    }

    private void CicleOfDay()
    {

        ciclingTimer += Time.deltaTime;
        if (isNight == false)
        {

            if (ciclingTimer >= timerToChange)
            {
                ciclingTimer = 0;
                isNight = true;
            }
        }
        else if (isNight == true)
        {
            if (ciclingTimer >= timerToChange)
            {
                ciclingTimer = 0;
                isNight = false;
            }
        }
    }
}