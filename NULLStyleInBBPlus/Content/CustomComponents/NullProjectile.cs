using DevTools;
using NULL.Content;
using NULL.NPCs;
using PixelInternalAPI.Extensions;
using UnityEngine;
using s = UnityEngine.SerializeField;

namespace NULL.CustomComponents {

    public class NullProjectile : MonoBehaviour {
        [s] GameObject visibleObject;
        [s] int initLayer;
        [s] public Vector3 spawnPoint;
        [s] bool held, thrown, hit;
        [s] float speed = 69f, life = 10f, val;
        BossManager Bm => BossManager.Instance;
        [s] Transform targetTransform;
        void Start() {
            if (name.Contains("Banana")) transform.localPosition += Vector3.up * 2.7f;
            spawnPoint = transform.position;

            try { visibleObject = transform.AllChilds().Find(x => x.GetComponent<SpriteRenderer>() != null || x.GetComponent<MeshRenderer>() != null).gameObject; }
            catch { visibleObject = gameObject; }
            initLayer = visibleObject.layer;
            targetTransform = Singleton<CoreGameManager>.Instance.GetCamera(0).transform;
        }
        void Throw() {
            thrown = true;
            transform.rotation = targetTransform.rotation;
            held = false;
            Bm.PlayerHasProjectile = false;
        }
        void OnTriggerEnter(Collider other) {
            if (thrown && !hit) {
                if (other.name.Contains("NULL") && !Bm.bossTransitionWaiting) {
                    hit = true;
                    other.GetComponent<NullNPC>().Hit(1);
                    Bm.NullHit(1);
                    Destroy(gameObject);
                }
            }
            if (!thrown && other.CompareTag("Player") && !Bm.PlayerHasProjectile) {
                held = true;
                Bm.PlayerHasProjectile = true;
            }
        }
        void Update() {
            if (Singleton<CoreGameManager>.Instance.Paused || visibleObject == null || Time.timeScale == 0) return;
            val += Time.deltaTime;

            if (held) {
                transform.position = targetTransform.transform.position + targetTransform.transform.forward * 5.5f - Vector3.up * (name.Contains("Banana") ? 2.25f : 4.25f);
                visibleObject.transform.localPosition += new Vector3(0f, Mathf.Sin(val * 5f) / 120, 0f);
                transform.localEulerAngles = targetTransform.localEulerAngles + (name.Contains("Chair") ? 90 : 0) * Vector3.up;

                visibleObject.layer = 29;
                if (Singleton<InputManager>.Instance.GetDigitalInput("Interact", true))
                    Throw();
            }
            else {
                visibleObject.transform.localPosition += new Vector3(0f, Mathf.Sin(val * 5f) / 145, 0f);
                visibleObject.layer = initLayer;
            }

            if (thrown) {
                transform.position += transform.forward * speed * Time.deltaTime * ExtraVariables.ec.EnvironmentTimeScale;
                life -= Time.deltaTime;
                if (life <= 0f)
                    Respawn();
            }
        }
        void Respawn() {
            thrown = false;
            held = false;
            life = 10f;
            transform.rotation = Quaternion.identity;
            transform.position = spawnPoint;
        }
    }
}