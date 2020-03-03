using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public static PlayerController current;

    public float Health {get { return health; } set { if (value < health) { GameManager.current.ClearHits(); } health = Mathf.Clamp(value, 0, maxHealth); UpdateHealth(); } }
    private float health;
    public float maxHealth;
    public float movementSpeed;
    public float timeToHeal = 10;

    public Scroller background1;
    public Scroller background2;
    public Transform starEmitter;

    private Vector3 lastPosition;

    private Vector3 topRight;
    private Vector3 bottomLeft;

    private Image healthBar;
    
    private float healthBarSize;
    private float lastTimeHit;
    public bool waitAtStart=true;

    private float horizontalMovement;
    private float verticalMovement;
    private bool alreadyMoving;

    private int repairKit;
    private int impactShields=3;
    private bool shield;
    public bool dodging;
    private GameObject forceField;
    private float left = 0;
    private float up = 0;
    private bool alreadyWaitingToHeal = false;

    private void UpdateHealth()
    {
        float percentage = (float)health / (float)maxHealth;
        healthBar.rectTransform.sizeDelta = new Vector2(healthBarSize * percentage, healthBar.rectTransform.rect.height);
        healthBar.rectTransform.position = new Vector3(Screen.width - healthBarSize / 2 + (healthBarSize * (1 - percentage) / 2) - 5, healthBar.rectTransform.position.y, healthBar.rectTransform.position.z);

        if (percentage >= 0.6f)
        {
            //Green bar to Yellow bar
            healthBar.color = new Color((1 - percentage) * 2.5f, 1, 0);
        }
        else
        {
            //Yellow bar to red bar
            healthBar.color = new Color(1, percentage / 0.6f, 0);
        }

        if (health < maxHealth )//AND NOT ALREADY HEALING
            StartCoroutine(HealAfterTime());

        //If the health reaches 0 or below, player is dead
        if (health <= 0)
        {
            GameManager.current.PlayerDied();
            GameManager.current.SpawnExplosion(transform.position);
            Destroy(gameObject);
        }

        if(MiscData.repairKits != 0)
        {
            repairKit -= 1;
        }
    }

    public void AddRepairKit()
    {
        repairKit += 1;
    }

    public void AddImpactShield()
    {
        impactShields += 1;
    }

    private IEnumerator HealAfterTime()
    {
        if (!alreadyWaitingToHeal)
        {
            while (health < maxHealth)
            {
                alreadyWaitingToHeal = true;
                if (Time.time - lastTimeHit >= timeToHeal)
                {
                    Health += movementSpeed * Time.deltaTime;
                }
                yield return new WaitForEndOfFrame();
            }
        }
        yield return null;
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener<float>("UpdateHorizontal", UpdateHorizontal);
        EventManager.Instance.RemoveEventListener<float>("UpdateVertical", UpdateVertical);
        EventManager.Instance.RemoveEventListener<bool>("UpdateImpact", UpdateImpact);
        EventManager.Instance.RemoveEventListener<bool>("UpdateRepair", UpdateRepair);
        EventManager.Instance.RemoveEventListener<bool>("UpdateDodge", UpdateDodge);
    }

    private void Awake()
    {
        current = this;

        //TopRight/BottomLeft for Background objects
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 15));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 15));
        //Gets distance between left side of the screen and right side of the screen
        float sizeVariable = Mathf.Abs(topRight.x - bottomLeft.x);

        background1.transform.localScale = background2.transform.localScale = new Vector3(sizeVariable, 1, sizeVariable * 2);
        background2.transform.position = background1.transform.position + new Vector3(0, 0, sizeVariable * 2);

        starEmitter.localScale = new Vector3(sizeVariable, 1, 1);
        starEmitter.GetComponent<ParticleSystem>().Stop();
        starEmitter.GetComponent<ParticleSystem>().Play();
        starEmitter.position = new Vector3(0, -2, background1.transform.position.z + Mathf.Abs(topRight.z - bottomLeft.z));

        background1.tileSize = background2.tileSize =  sizeVariable * 2;

        //TopRight/BottomLeft for player boundaries
        topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,0, 10));
        healthBar = GameManager.current.healthBar;
        healthBarSize = healthBar.rectTransform.rect.width;

        lastPosition = new Vector3(0, 0, -10);

        PlayerInputManager fake = PlayerInputManager.Instance;
        EventManager.Instance.AddEventListener<float>("UpdateHorizontal", UpdateHorizontal);
        EventManager.Instance.AddEventListener<float>("UpdateVertical", UpdateVertical);
        EventManager.Instance.AddEventListener<bool>("UpdateImpact", UpdateImpact);
        EventManager.Instance.AddEventListener<bool>("UpdateRepair", UpdateRepair);
        EventManager.Instance.AddEventListener<bool>("UpdateDodge", UpdateDodge);

        StartCoroutine(CutsceneManager.Instance.ReadCutscene(true));


        if(MiscData.repairKits > 0)
        {
            repairKit = 1;
        }
        else
        {
            GameManager.current.UseRepairKit();
        }
    }

    public void EndLevel()
    {
        StartCoroutine(CutsceneManager.Instance.ReadCutscene(false));
    }

    private void UpdateHorizontal(float horizontal)
    {
        horizontalMovement = horizontal;
        if(!alreadyMoving && !waitAtStart)
            StartCoroutine(Move());
    }
    private void UpdateVertical(float vertical)
    {
        verticalMovement = vertical;
        if (!alreadyMoving && !waitAtStart)
            StartCoroutine(Move());
    }

    private void UpdateImpact(bool impact)
    {
        if(impact && impactShields != 0 && !shield && !waitAtStart)
        {
            impactShields -= 1;
            shield = true;
            forceField = GameManager.current.EnableImpactField();
            StartCoroutine(UseImpactShield());
        }
    }
    private void UpdateRepair(bool repair)
    {
        if(repair && repairKit != 0 && !waitAtStart)
        {
            repairKit -= 1;
            StartCoroutine(UseRepairKit());
        }
    }

    private void UpdateDodge(bool dodge)
    {
        if(!dodging && dodge && !waitAtStart)
        {
            dodging = true;
            StartCoroutine(DoDodge());
        }
    }

    IEnumerator DoDodge()
    {
        dodging = true;
        float currentTime = 0;
        up = verticalMovement;
        left = horizontalMovement;
        if (movementSpeed > 20)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
                renderer.enabled = false;
            while(currentTime < .5f)
            {
                currentTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            foreach (Renderer renderer in renderers)
                renderer.enabled = true;
        }
        else
        {
            float direction = horizontalMovement;
            while (currentTime < 0.5f)
            {
                currentTime += Time.deltaTime;
                if(direction <= 0)
                    transform.rotation = Quaternion.Euler(0, 0, currentTime * 720);
                else
                    transform.rotation = Quaternion.Euler(0, 0, -currentTime * 720);
                yield return new WaitForEndOfFrame();
            }
        }
        dodging = false;
        yield return null;
    }

    IEnumerator UseImpactShield()
    {
        float time = 0;
        while(time < 1)
        {
            time += Time.deltaTime*2;
            forceField.transform.localScale = Vector3.Lerp(new Vector3(), new Vector3(2.5f, 2.5f, 2.5f), time);
            yield return new WaitForEndOfFrame();
        }
        time = 0;
        while(time < 5)
        {
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        time = 0;
        while(time < 2)
        {
            time += Time.deltaTime;
            float r = Random.value;
            if (r < 0.5f)
                forceField.transform.localScale = new Vector3();
            else
                forceField.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            yield return new WaitForEndOfFrame();
        }
        shield = false;
        Destroy(forceField);
        yield return null;
    }

    IEnumerator UseRepairKit()
    {
        float amountToHeal = maxHealth - health;
        float current = 0;
        GameManager.current.UseRepairKit();
        while(current < amountToHeal)
        {
            current += 1;
            Health += 1;
            yield return new WaitForEndOfFrame();
        }
        MiscData.repairKits -= 1;
        yield return null;
    }

    IEnumerator ShieldHit()
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(0.25f);
        Time.timeScale = 1;
        yield return null;
    }

    IEnumerator Move()
    {
        alreadyMoving = true;
        float moveByTime = movementSpeed * Time.deltaTime;
        lastPosition += (Vector3.forward * verticalMovement) * moveByTime + (Vector3.right * horizontalMovement) * moveByTime;

        while (transform.position != lastPosition && !waitAtStart)
        {
            if (Time.timeScale != 0)
            {
                moveByTime = movementSpeed * Time.deltaTime;
                //Choose direction to move towards based on previously inputted direction
                lastPosition += (Vector3.forward * verticalMovement) * moveByTime + (Vector3.right * horizontalMovement) * moveByTime;

                bool hadToStop = false;

                if (lastPosition.x < bottomLeft.x + 1 || lastPosition.x > topRight.x - 1)
                {
                    lastPosition -= Vector3.right * horizontalMovement * moveByTime;
                    hadToStop = true;
                }
                if (lastPosition.z < bottomLeft.z + 0.5f || lastPosition.z > topRight.z - 0.5f)
                {
                    lastPosition -= Vector3.forward * verticalMovement * moveByTime;
                }

                //Move in that direction floatily
                transform.position = Vector3.Lerp(transform.position, lastPosition, 1 / movementSpeed);

                if (!dodging)
                {
                    if (horizontalMovement == 0 || hadToStop)
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, 0), 0.1f);
                    else
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -25 * horizontalMovement), 0.1f);
                }
            }
            yield return new WaitForEndOfFrame();
        }
        alreadyMoving = false;
        yield return null;
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.GetComponent<Hazard>() && collider.GetComponent<Hazard>().team != gameObject.tag && !dodging)
        {
            Hazard hazard = collider.GetComponent<Hazard>();
            CameraShake.current.Shake(hazard.shakeAmount, hazard.timeShake);
            if (!shield)
            {
                lastTimeHit = Time.time;
                Health -= hazard.damage;
            }
            else
            {
                hazard.playerDestroyed = true;
                StartCoroutine(ShieldHit());
            }
            GameManager.current.SpawnExplosion(collider.transform.position);
            Destroy(collider.gameObject);
        }

        if (collider.GetComponent<Collect>())
        {
            collider.GetComponent<Collect>().DoThing();
        }
    }
}
