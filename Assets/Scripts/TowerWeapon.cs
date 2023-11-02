using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public enum WeaponState { SearchTarget = 0, AttackToTarget }

public class TowerWeapon : MonoBehaviour
{
    [SerializeField]
    private TowerTemplate towerTemplate;        //타워 정보 (공격력, 공격속도 등)
    [SerializeField]
    private GameObject projectilePrefab;        //발사체 프리팹
    [SerializeField]
    private Transform spawnPoint;               //발사체 생성 위치
    //[SerializeField]
    //private float attackRate = 0.5f;            //공격 속도
    //[SerializeField]
    //private float attackRange = 2.0f;           //공격 범위
    //[SerializeField]
    //private int attackDamage = 1;               //공격력
    private int level = 0;                      //타워 레벨
    private WeaponState weaponState = WeaponState.SearchTarget; //타워 무기의 상태
    private Transform attackTarget = null;      //공격 대상
    private SpriteRenderer spriteRenderer;      //타워 오브젝트  이미지 변경용
    private EnemySpawner enemySpawner;
    private PlayerGold playerGold;              //플레이어의 골드 정보 획득 및 설정
    private Tile ownerTile;                     //현재 타워가 배치되어 있는 타일

    //public float Damage => attackDamage;
    //public float Rate => attackRate;
    //public float Range => attackRange;
    public Sprite TowerSprite => towerTemplate.weapon[level].sprite;
    public float Damage => towerTemplate.weapon[level].damage;
    public float Rate => towerTemplate.weapon[level].rate;
    public float Range => towerTemplate.weapon[level].range;
    public int Level => level + 1;
    public int MaxLevel => towerTemplate.weapon.Length;

    public void Setup(EnemySpawner enemySpawner, PlayerGold playerGold, Tile ownerTile)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        this.enemySpawner = enemySpawner;
        this.playerGold = playerGold;
        this.ownerTile = ownerTile;
        
        //최초 상태를 WeaponState.SearchTarget으로 설정
        ChangeState(WeaponState.SearchTarget);
    }

    public void ChangeState(WeaponState newState)
    {
        //이전에 재생중이던 상태 종료
        StopCoroutine(weaponState.ToString());
        //상태 변경
        weaponState = newState;
        //새로운 상태 재생
        StartCoroutine(weaponState.ToString());
    }

    private void Update()
    {
        if(attackTarget != null)
        {
            RotateToTarget();
        }
    }

    private void RotateToTarget()
    {
        //원점으로부터의 거리와 수평축으로부터의 각도를 이용해 위치를 구하는 극 좌표계 이요
        //각도 = arctan(y/x)
        //x, y 변위값 구하기
        float dx = attackTarget.position.x - transform.position.x;
        float dy = attackTarget.position.y - transform.position.y;
        //x,y 변위값을 바탕으로 각도 구하기
        //각도가 radian 단위이기 때문에 Mathf.Rad2Deg를 곱해 도 단위를 구함
        float degree = Mathf.Atan2(dy,dx) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0,0,degree);
    }

    private IEnumerator SearchTarget()
    {
        while(true)
        {
            //제일 가까이 있는 적을 찾기 위해 최초 거리를 최대한 크게 설정
            float closestDistSqr = Mathf.Infinity;
            //EnemySpawner의 EnemyList에 있는 현재 맵에 존재하는 모든 적 검사
            for(int i = 0;i<enemySpawner.EnemyList.Count;++i)
            {
                float distance = Vector3.Distance(enemySpawner.EnemyList[i].transform.position, transform.position);
                //현재 검사중인 적과의 거리가 공격범위 내에 있고, 현재까지 검사한 적보다 거리가 가까우면
                if(distance <= Range && distance <= closestDistSqr)
                {
                    closestDistSqr = distance;
                    attackTarget = enemySpawner.EnemyList[i].transform;
                }
            }

            if(attackTarget != null)
            {
                ChangeState(WeaponState.AttackToTarget);
            }

            yield return null;
        }
    }
    
    private IEnumerator AttackToTarget()
    {
        while(true)
        {
            //1, target이 있는지 검사 (다른 발사체에 의해 제거, Goal 지점까지 이동해 삭제 등)
            if(attackTarget == null)
            {
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            //2. target이 공격 범위안에 있는지 검사 (공격범위를 벗어나면 새로운 적 탐색)
            float distance = Vector3.Distance(attackTarget.position, transform.position);
            if(distance>Range)
            {
                attackTarget = null;
                ChangeState(WeaponState.SearchTarget);
                break;
            }

            //3. attackrate 시간만큼 대기
            yield return new WaitForSeconds(Rate);

            //4. 공격 (발사체 생성)
            SpawnProjectile();
        }
    }    

    private void SpawnProjectile()
    {
        GameObject clone = Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        //생성된 발사체에게 공격대상(attackTarget) 정보 제공
        clone.GetComponent<Projectile>().Setup(attackTarget, Damage);
    }

    public bool Upgrade()
    {
        //타워 업그레이드에 필요한 골드가 충분한지 검사
        if(playerGold.CurrentGold< towerTemplate.weapon[level+1].cost)
        {
            return false;
        }

        //타워 레벨 증가
        level++;
        //타워 외형 변경 (Sprite)
        spriteRenderer.sprite = towerTemplate.weapon[level].sprite;
        //골드 차감
        playerGold.CurrentGold -= towerTemplate.weapon[level].cost;

        return true;
    }

    public void Sell()
    {
        //골드 증가
        playerGold.CurrentGold += towerTemplate.weapon[level].sell;
        //현재 타일에 다시 타워 건설이 가능하도록 설정
        ownerTile.IsBuildTower = false;
        //타워 파괴
        Destroy(gameObject);
    }
}
