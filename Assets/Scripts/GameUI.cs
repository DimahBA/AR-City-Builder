using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    public UIDocument UIDoc;
    private Label moneyLabel;
    private Label populationLabel;
    private Label dayLabel;
    private ProgressBar dayProgressBar;

    private int currentMoney = 7000;
    private int currentPopulation = 0;
    private int currentDay = 0;
    
    private void Start()
{
    var root = UIDoc.rootVisualElement;

    moneyLabel = root.Q<Label>("MoneyLabel");
    populationLabel = root.Q<Label>("DensityLabel");
    dayLabel = root.Q<Label>("DayLabel");
    dayProgressBar = root.Q<ProgressBar>("DayProgressBar");

    if (dayProgressBar != null)
    {
        // Set the range to 0-1 to match your progress values
        dayProgressBar.lowValue = 0f;
        dayProgressBar.highValue = 1f;
        dayProgressBar.value = 0f;
    }
    else
    {
        Debug.LogError("DayProgressBar not found in UI Document!");
    }

    UpdateMoneyDisplay();
    UpdatePopulationDisplay();
    UpdateDayDisplay();
    UpdateDayProgress(0f);
}




    public void UpdateDayProgress(float progress)
    {
        if (dayProgressBar != null)
        {
            dayProgressBar.value = Mathf.Clamp01(progress);
        }
    }

    
    void UpdateMoneyDisplay()
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = "$" + currentMoney.ToString("N0");
        }
    }

    void UpdatePopulationDisplay()
    {
        if (populationLabel != null)
        {
            populationLabel.text = currentPopulation.ToString("N0");
        }
    }
    
    void UpdateDayDisplay()
    {
        if (dayLabel != null)
        {
            dayLabel.text = "Day " + currentDay;
        }
    }
    
    public bool CanAfford(int cost)
    {
        return currentMoney >= cost;
    }
    

    

    public void SpendMoney(int amount)
    {
        
        currentMoney -= amount;
        UpdateMoneyDisplay();
    }
    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyDisplay();
    }
    
    public int GetCurrentMoney()
    {
        return currentMoney;
    }
    
    public void UpdatePopulation(int population)
    {
        currentPopulation = population;
        UpdatePopulationDisplay();
    }
    
    public void UpdateDayDisplay(int day)
    {
        currentDay = day;
        UpdateDayDisplay();
    }
    
    public int GetCurrentPopulation()
    {
        return currentPopulation;
    }
}
