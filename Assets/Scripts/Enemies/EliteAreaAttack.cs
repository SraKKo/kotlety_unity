using UnityEngine;

namespace RetyKotlety {
public class EliteAreaAttack : MonoBehaviour {
    const int Terrain = ~((1 << 8) | (1 << 9) | (1 << 11));
    const int Segments = 48;
    static Material ringMaterial;
    Enemy enemy;
    LineRenderer ring;
    Vector3 centre;
    float releaseAt;
    bool mortar;
    public bool Charging => ring && ring.enabled;
    public float Range => mortar ? 20f : 4.5f;
    public float Cooldown => mortar ? 5f : 3.6f;
    float Radius => mortar ? 3.4f : 4.5f;
    Color Tint => mortar ? new Color(.83f,.35f,1f) : new Color(1f,.45f,.12f);

    public static float SizeMultiplier(bool isMortar) => isMortar ? 1.2f : 1.45f;

    public void Configure(Enemy owner, bool isMortar) {
        enemy = owner;
        mortar = isMortar;
        transform.localScale *= SizeMultiplier(mortar);
        var paint = new MaterialPropertyBlock();
        foreach (var renderer in GetComponentsInChildren<Renderer>()) {
            renderer.GetPropertyBlock(paint);
            paint.SetColor("_Color", Tint);
            renderer.SetPropertyBlock(paint);
        }
        ring = new GameObject("Obszar ataku").AddComponent<LineRenderer>();
        ring.transform.SetParent(transform, false);
        if (!ringMaterial) ringMaterial = new Material(Shader.Find("Sprites/Default"));
        ring.sharedMaterial = ringMaterial;
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = Segments;
        ring.startColor = ring.endColor = Tint;
        ring.enabled = false;
    }

    public void Begin(PlayerController player) {
        Vector3 target = mortar ? player.transform.position : transform.position;
        if (!Physics.Raycast(target + Vector3.up * .3f, Vector3.down, out var ground, 15f, Terrain, QueryTriggerInteraction.Ignore)) return;
        centre = ground.point + ground.normal * .07f;
        var plane = Quaternion.FromToRotation(Vector3.up, ground.normal);
        for (int i = 0; i < Segments; ++i) {
            float angle = i * Mathf.PI * 2 / Segments;
            ring.SetPosition(i, centre + plane * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * Radius);
        }
        ring.enabled = true;
        releaseAt = Time.time + (mortar ? 1.5f : 1.1f);
    }

    void Update() {
        if (!Charging) return;
        var session = GameSession.Instance;
        if (!session || !session.player || enemy.health <= 0 || enemy.IsAirborne) {
            ring.enabled = false;
            return;
        }
        if (session.paused) return;
        ring.widthMultiplier = .13f + .12f * Mathf.PingPong(Time.time * 5, 1);
        if (Time.time < releaseAt) return;
        ring.enabled = false;
        for (int i = 0; i < 4; i++) Effect.Emit(ring.GetPosition(i * Segments / 4), Vector3.up, ImpactEffect.Bounce);
        var player = session.player;
        // Match the floor warning to the player's feet; jumping onto a platform clears the blast.
        var capsule = player.GetComponent<CharacterController>();
        var offset = capsule.bounds.center - centre;
        bool inside = offset.x * offset.x + offset.z * offset.z <= Radius * Radius;
        if (inside && Mathf.Abs(capsule.bounds.min.y - centre.y) < 1.6f &&
            !Physics.Linecast(centre + Vector3.up * .4f, capsule.bounds.center, Terrain, QueryTriggerInteraction.Ignore))
            player.Damage(mortar ? 20 : 26);
    }
}
}
