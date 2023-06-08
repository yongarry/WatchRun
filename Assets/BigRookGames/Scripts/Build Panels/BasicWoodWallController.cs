using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace BigRookGames.Build
{
    public class BasicWoodWallController : MonoBehaviour
    {
        Animator anim;
        [SerializeField] private int m_Health = 100;
        public GameObject wallExplosionPrefab;
        public Transform wallExplosionPosition;
        public bool alive = true;
        public AudioClip spawnClip, damageClip, deathClip;
        public AudioSource damageAudioSource;
       
        public GameObject Player;
        public GameObject rootWallObj;

        // --- Variable to determine when to play damage animations and audio ---
        // --- Stage 0 - health: Full Health
        // --- Stage 1 - health: 71-99
        // --- Stage 2 - health: 21-70
        // --- Stage 3 - health: 1-20
        // --- Stage 4 - health: 0;
        public int panelStage = 0;


        private Animator an_Player;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            damageAudioSource = GetComponent<AudioSource>();
            an_Player = Player.GetComponent<Animator>();
            if (!alive) gameObject.SetActive(false);
        }

        void Start()
        {
            damageAudioSource.clip = spawnClip;
            damageAudioSource.Play();
        }


        private void Update()
        {
            // --- If alive and the health has crossed below the next threshold, update panel ---
            if (alive && CheckStageHealthThreshold())
            {
                anim.SetInteger("Health", m_Health);
                if (m_Health <= 0)
                {
                    alive = false;
                    damageAudioSource.clip = deathClip;
                    damageAudioSource.Play();
                    Instantiate(wallExplosionPrefab, wallExplosionPosition);
                }
                else
                {
                    damageAudioSource.clip = spawnClip;
                    damageAudioSource.Play();
                }
            }

            // --- For Example Scene, Take Out In Real Project (set health to 100 to reset wall) ---
            else if (!alive)
            {
                if (m_Health == 100)
                {
                    alive = true;
                }
            }
        }

        private bool CheckStageHealthThreshold()
        {
            switch(panelStage)
            {
                case 0:
                    if (m_Health < 100)
                    {
                        panelStage++;
                        return true;
                    }
                    break;
                case 1:
                    if (m_Health < 71)
                    {
                        panelStage++;
                        return true;
                    }
                    break;
                case 2:
                    if (m_Health < 21)
                    {
                        panelStage++;
                        return true;
                    }
                    break;
                case 3:
                    if (m_Health <= 0)
                    {
                        panelStage++;
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Sets the Health integer parameter on the animator to play each animation.
        /// This is called from the UI buttons in the example scene.
        /// </summary>
        /// <param name="damageStage"></param>
        public void PlayDamageAnimation(int damageStage)
        {
            switch (damageStage)
            {
                case 0:
                    m_Health = 100;
                    panelStage = 0;
                    break;
                case 1:
                    m_Health = 99;
                    break;
                case 2:
                    m_Health = 70;
                    break;
                case 3:
                    m_Health = 20;
                    break;
                case 4:
                    m_Health = 0;
                    break;
            }
            anim.SetInteger("Health", m_Health);
        }
    

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Hand")
            {
                switch (m_Health)
                {
                    case 100:
                        PlayDamageAnimation(1);
                        break;
                    case 99:
                        PlayDamageAnimation(2);
                        break;
                    case 70:
                        PlayDamageAnimation(3);
                        break;
                    case 20:
                        PlayDamageAnimation(4);
                        an_Player.SetLayerWeight(1,0);
                        DestroyWall();
                        // GameObject duplicate = Instantiate(this.transform.parent.gameObject);
                        // Vector3 temp = new Vector3(0,0,40);
                        // duplicate.gameObject.transform.position += temp;

                        // Destroy(this.transform.parent.gameObject, 1);
                        break;
                    // case 0:
                    //     an_Player.SetLayerWeight(1,0);
                    //     Destroy(gameObject, 4);
                    //     break;
                }
                // anim.SetInteger("Health", m_Health);
            }
        }

        void DestroyWall()
        {
            // gameObject.SetActive(false);
            // m_Health = 100;
            // anim.SetInteger("Health", m_Health);
            StartCoroutine(refillWall());
            // StartCoroutine(destroyWall());
            // Destroy(this.transform.parent.gameObject, 10);
        }

        IEnumerator refillWall()
        {
            yield return new WaitForSeconds(4);
            PlayDamageAnimation(0);
            StartCoroutine(destroyWall());
            Destroy(this.transform.parent.gameObject, 10);
        }

        IEnumerator destroyWall()
        {
            yield return new WaitForSeconds(3);
            GameObject duplicate = Instantiate(this.transform.parent.gameObject);
            Vector3 temp = new Vector3(0,0,60);
            duplicate.gameObject.transform.position += temp;
        }

    }
}