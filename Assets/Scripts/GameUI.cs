using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    public UIDocument UIDoc;
    private Label moneyLabel;
    private int currentMoney = 100000;
    
    private void Start()
    {
        var root = UIDoc.rootVisualElement;
        moneyLabel = root.Q<Label>("MoneyLabel");
        UpdateMoneyDisplay();
    }
    
    void UpdateMoneyDisplay()
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = "$" + currentMoney.ToString("N0");
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