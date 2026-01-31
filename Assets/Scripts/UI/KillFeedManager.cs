using UnityEngine;
using System.Collections;

/// <summary>
/// Kill Feed Manager - Hiển thị thông báo ai giết ai
/// </summary>
public class KillFeedManager : MonoBehaviour
{
    public static KillFeedManager Instance;
    
    [Header("UI")]
    public Transform killFeedContainer;
    public GameObject killFeedItemPrefab;
    
    [Header("Settings")]
    public float displayDuration = 5f;
    public int maxItems = 5;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void ShowKill(string killerName, string victimName, string weaponName = "")
    {
        if (killFeedItemPrefab == null || killFeedContainer == null) return;
        
        // Tạo kill feed item mới
        GameObject item = Instantiate(killFeedItemPrefab, killFeedContainer);
        
        // Set text
        TMPro.TextMeshProUGUI text = item.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            if (string.IsNullOrEmpty(weaponName))
                text.text = $"<color=red>{killerName}</color> eliminated <color=yellow>{victimName}</color>";
            else
                text.text = $"<color=red>{killerName}</color> [{weaponName}] <color=yellow>{victimName}</color>";
        }
        
        // Remove sau một thời gian
        StartCoroutine(RemoveItemAfterDelay(item, displayDuration));
        
        // Giới hạn số lượng items
        if (killFeedContainer.childCount > maxItems)
        {
            Destroy(killFeedContainer.GetChild(0).gameObject);
        }
    }
    
    IEnumerator RemoveItemAfterDelay(GameObject item, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (item != null)
        {
            // Fade out effect
            CanvasGroup cg = item.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = item.AddComponent<CanvasGroup>();
            
            float fadeTime = 0.5f;
            float elapsed = 0f;
            
            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                cg.alpha = 1f - (elapsed / fadeTime);
                yield return null;
            }
            
            Destroy(item);
        }
    }
}
