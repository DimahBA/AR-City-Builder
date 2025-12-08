using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    public UIDocument UIDoc;
    private Label moneyLabel;
    private Label populationLabel;
    private int currentMoney = 100000;
    private int currentPopulation = 0;
    
    private void Start()
    {
        var root = UIDoc.rootVisualElement;
        moneyLabel = root.Q<Label>("MoneyLabel");
        populationLabel = root.Q<Label>("DensityLabel");

        UpdateMoneyDisplay();
        UpdatePopulationDisplay();

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
}