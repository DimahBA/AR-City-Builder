using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    public UIDocument UIDoc;
    private Label moneyLabel;
    private Label populationLabel;
    private Label dayLabel;
    private int currentMoney = 100000;
    private int currentPopulation = 0;
    private int currentDay = 0;
    
    private void Start()
    {
        var root = UIDoc.rootVisualElement;
        moneyLabel = root.Q<Label>("MoneyLabel");
        populationLabel = root.Q<Label>("DensityLabel");
        dayLabel = root.Q<Label>("DayLabel");

        UpdateMoneyDisplay();
        UpdatePopulationDisplay();
        UpdateDayDisplay();
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
        Debug.LogError($"[SPENDING MONEY] Amount: ${amount}");
        Debug.LogError($"Stack Trace:\n{System.Environment.StackTrace}");
        
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
