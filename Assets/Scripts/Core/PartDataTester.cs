using UnityEngine;

public class PartDataTester : MonoBehaviour
{
    [Header("Test Part")]
    public PartData testPart;
    
    void Start()
    {
        if (testPart != null)
        {
            Debug.Log($"Part: {testPart.partName}");
            Debug.Log($"Type: {testPart.type}");
            Debug.Log($"HP Bonus: {testPart.hpBonus}");
            Debug.Log($"Attack Bonus: {testPart.attackBonus}");
            Debug.Log($"Description: {testPart.description}");
        }
        else
        {
            Debug.Log("No test part assigned to PartDataTester!");
        }
    }
}