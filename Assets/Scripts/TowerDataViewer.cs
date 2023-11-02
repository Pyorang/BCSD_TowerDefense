using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerDataViewer : MonoBehaviour
{
    [SerializeField]
    private Image imageTower;
    [SerializeField]
    private TextMeshProUGUI textDamage;
    [SerializeField]
    private TextMeshProUGUI textRate;
    [SerializeField]
    private TextMeshProUGUI textRange;
    [SerializeField]
    private TextMeshProUGUI textLevel;
    [SerializeField]
    private TowerAttackRange towerAttackRange;
    [SerializeField]
    private Button buttonUpgrade;
    [SerializeField]
    private SystemTextViewer systemTextViewer;

    private TowerWeapon currentTower;

    private void Awake()
    {
        OffPanel();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OffPanel();
        }
    }

    public void OnPanel(Transform towerWeapon)
    {
        //출력해야 하는 타워 정보를 받아와서 저장
        currentTower = towerWeapon.GetComponent<TowerWeapon>();
        // 타워 정보 Panel On
        gameObject.SetActive(true);
        //타워 정보를 갱신
        UpdateTowerData();
        //타워 오브젝트 주변에 표시되는 타워 공격범위 Sprite On
        towerAttackRange.OnAttackRange(currentTower.transform.position, currentTower.Range);
    }

    public void OffPanel()
    {
        //타워 정보 Panel Off
        gameObject.SetActive(false);
        //타워 공격범위 Sprite Off
        towerAttackRange.OffAttackRange();
    }

    private void UpdateTowerData()
    {
        imageTower.sprite = currentTower.TowerSprite;
        textDamage.text = "Damage : " + currentTower.Damage;
        textRate.text = "Rate : " + currentTower.Rate;
        textRange.text = "Range : " + currentTower.Range;
        textLevel.text = "Level : " + currentTower.Level;

        //업그레이드가 불가능해지면 버튼 비활성화
        buttonUpgrade.interactable = currentTower.Level < currentTower.MaxLevel ? true : false; 
    }

    public void OnClickEventTowerUpgrade()
    {
        //타워 업그레이드 시도 (성공 : true, 실패 : false)
        bool isSuccess = currentTower.Upgrade();

        if (isSuccess)
        {   
            //타워가 업그레이드 되었기 때문에 타워 정보 갱신
            UpdateTowerData();
            //타워 주변에 보이는 공격범위도 갱신
            towerAttackRange.OnAttackRange(currentTower.transform.position, currentTower.Range);
        }
        else
        {
            //타워 업그레이드에 필요한 비용이 부족하다고 출력
            systemTextViewer.PrintText(SystemType.Money);
        }
    }

    public void OnClickEventTowerSell()
    {
        //타워 판매
        currentTower.Sell();
        //선택한 타워가 사라져서 Panel, 공격범위 off
        OffPanel();
    }    
}
