using UnityEngine;
namespace RetyKotlety { public class ExitArea : MonoBehaviour { void OnTriggerEnter(Collider other){if(other.GetComponent<PlayerController>())GameSession.Instance.Win();} } }
