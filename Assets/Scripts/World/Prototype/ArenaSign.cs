using UnityEngine;
namespace RetyKotlety {
[ExecuteAlways,RequireComponent(typeof(TextMesh))]
public class ArenaSign : MonoBehaviour {
 void OnEnable(){var label=GetComponent<TextMesh>();var font=Resources.Load<Font>("UI/PixelAE-Bold");if(!font)return;label.font=font;GetComponent<MeshRenderer>().sharedMaterial=font.material;}
}
}
