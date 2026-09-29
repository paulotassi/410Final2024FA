using System.Collections.Generic;
using UnityEngine;

// Ammo for the witch's normal shots: three familiars orbit her. The next one to be fired floats where she is
// aiming (it replaces her single aim familiar). Firing sends it out as the projectile; the next familiar swoops
// into the aim position. A spent familiar flies back slowly, so when to shoot is a decision.
public class FamiliarLauncher : MonoBehaviour
{
    [Header("Familiars")]
    public int maxFamiliars = 3;
    [Tooltip("Size relative to the witch's own aim familiar")]
    public float sizeMultiplier = 1f;
    public float orbitRadius = 1.6f;
    [Tooltip("Size of the familiars waiting their turn, relative to the ready one")]
    public float waitingSize = 0.7f;
    [Tooltip("Tint of the familiars waiting their turn")]
    public Color waitingColor = new Color(0.45f, 0.6f, 1f, 0.75f);
    [Tooltip("Tint of the familiar that fires next")]
    public Color readyColor = new Color(1f, 0.92f, 0.45f, 1f);
    [Tooltip("How much the ready familiar gently pulses in size (0 = none)")]
    [Range(0f, 0.3f)]
    public float readyPulse = 0.08f;
    [Tooltip("Soft glowing ring drawn behind the familiar that fires next")]
    public bool showGlowRing = true;
    [Tooltip("Ring colour (RGBA) at the lowest point of the pulse")]
    public Color ringColorLow = new Color(1f, 0.85f, 0.3f, 0.2f);
    [Tooltip("Ring colour (RGBA) at the peak of the pulse")]
    public Color ringColorPeak = new Color(1f, 0.85f, 0.3f, 0.45f);
    [Tooltip("Ring diameter relative to the familiar's size")]
    public float ringSize = 1.9f;
    public float orbitSpeed = 60f;      // degrees per second
    [Tooltip("How quickly docked familiars glide to their spot")]
    public float followSpeed = 14f;

    [Header("Flight")]
    [Tooltip("Delay between two shots")]
    public float fireInterval = 0.6f;
    [Tooltip("How far a familiar flies before it turns around")]
    public float maxRange = 14f;
    [Tooltip("Speed of the flight home in units/second. Low = a shot is a real commitment")]
    public float returnSpeed = 5f;
    [Tooltip("Pause on impact before the familiar heads home")]
    public float hitPause = 0.25f;

    private PlayerController controller;
    private GameObject visualTemplate;
    private Transform aim;                 // the witch's existing aim familiar (position + rotation of the ready familiar)
    private SpriteRenderer aimRenderer;
    private Vector3 worldScale = Vector3.one;
    private readonly List<Transform> docks = new List<Transform>();
    private bool[] slotAway;
    private float[] pop;
    private float[] sizeNow;
    private Transform ring;
    private SpriteRenderer ringRenderer;
    private float angle;

    public int Available { get; private set; }
    public bool HasAmmo => Available > 0;

    // Rapid Fire upgrades speed the familiars up (and so shorten the reload)
    public float SpeedMultiplier => controller != null ? controller.familiarSpeedMultiplier : 1f;
    public float WorldSize => worldScale.x;

    public void Init(PlayerController owner, GameObject familiarVisual)
    {
        controller = owner;
        visualTemplate = familiarVisual;
        slotAway = new bool[maxFamiliars];
        pop = new float[maxFamiliars];
        sizeNow = new float[maxFamiliars];
        for (int i = 0; i < maxFamiliars; i++) sizeNow[i] = 1f;
        Available = maxFamiliars;

        if (familiarVisual != null)
        {
            aim = familiarVisual.transform;
            aimRenderer = familiarVisual.GetComponent<SpriteRenderer>();
            worldScale = new Vector3(Mathf.Abs(aim.lossyScale.x), Mathf.Abs(aim.lossyScale.y), 1f) * sizeMultiplier;
        }
        else
        {
            worldScale = Vector3.one * sizeMultiplier;
        }

        for (int i = 0; i < maxFamiliars; i++)
        {
            Transform dock = CreateVisual("FamiliarDock" + i, null).transform;
            dock.position = transform.position;
            docks.Add(dock);
        }

        if (showGlowRing) CreateRing();

        // the ready familiar replaces the single aim familiar
        if (aimRenderer != null) aimRenderer.enabled = false;
    }

    // A copy of the witch's familiar sprite (and its animation) with everything else stripped off
    public GameObject CreateVisual(string objectName, Transform parent)
    {
        GameObject visual;
        if (visualTemplate != null)
        {
            visual = Instantiate(visualTemplate, parent);
            foreach (Component c in visual.GetComponents<Component>())
            {
                if (c is Transform || c is SpriteRenderer || c is Animator) continue;
                Destroy(c);
            }
            foreach (Transform child in visual.transform) Destroy(child.gameObject);
            visual.SetActive(true);
        }
        else
        {
            // No familiar art found - a small circle so it is still visible
            visual = new GameObject();
            visual.transform.SetParent(parent, false);
            Texture2D white = Texture2D.whiteTexture;
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = Sprite.Create(white, new Rect(0, 0, white.width, white.height), new Vector2(0.5f, 0.5f), white.width / 0.5f);
            sr.color = new Color(1f, 0.85f, 0.4f);
        }

        visual.name = objectName;
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;   // the template's renderer is hidden, its copies must not be
            renderer.sortingOrder += 5;
        }

        // keep the same size in the world no matter what the parent's scale is
        float parentScale = parent != null ? Mathf.Abs(parent.lossyScale.x) : 1f;
        visual.transform.localScale = worldScale / Mathf.Max(0.0001f, parentScale);
        return visual;
    }

    // A soft ring (generated in code) that sits behind the ready familiar
    private void CreateRing()
    {
        const int n = 64;
        Texture2D tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n * 0.5f, n * 0.5f)) / (n * 0.5f);
                float edge = Mathf.Clamp01(1f - Mathf.Abs(r - 0.78f) / 0.2f);       // bright band near the rim
                float glow = Mathf.Clamp01(1f - r) * 0.35f;                           // faint fill inside
                float a = Mathf.Max(edge * edge, glow) * (r <= 1f ? 1f : 0f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();

        GameObject go = new GameObject("FamiliarReadyRing");
        ring = go.transform;
        ringRenderer = go.AddComponent<SpriteRenderer>();
        ringRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);   // 1 world unit wide at scale 1
        ringRenderer.sortingOrder = docks.Count > 0 && docks[0].GetComponent<SpriteRenderer>() != null ? docks[0].GetComponent<SpriteRenderer>().sortingOrder - 1 : 4;
        ringRenderer.color = ringColorLow;
        go.SetActive(false);
    }

    private int ReadySlot()
    {
        for (int i = 0; i < slotAway.Length; i++) if (!slotAway[i]) return i;
        return -1;
    }

    void LateUpdate()
    {
        if (docks.Count == 0) return;

        angle += orbitSpeed * Time.deltaTime;
        int ready = ReadySlot();
        int orbiting = Available - (ready >= 0 ? 1 : 0);
        int orbitIndex = 0;
        float follow = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        UpdateRing(ready);

        for (int i = 0; i < docks.Count; i++)
        {
            Transform d = docks[i];
            if (d == null) continue;
            if (slotAway[i]) { d.gameObject.SetActive(false); continue; }
            d.gameObject.SetActive(true);

            Vector3 target;
            if (i == ready)
            {
                // floats where the witch is aiming
                target = aim != null ? aim.position : transform.position + Vector3.right * orbitRadius;
                if (aim != null) d.rotation = aim.rotation;
            }
            else
            {
                float a = (angle + orbitIndex * 360f / Mathf.Max(1, orbiting)) * Mathf.Deg2Rad;
                target = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
                d.rotation = Quaternion.identity;
                orbitIndex++;
            }
            d.position = Vector3.Lerp(d.position, target, follow);

            // pop when it has just come home, keeping the witch's facing (mirrored scale) if she is flipped
            pop[i] = Mathf.MoveTowards(pop[i], 0f, Time.deltaTime * 2.5f);
            float sx = aim != null && aim.lossyScale.x < 0 ? -1f : 1f;
            sizeNow[i] = Mathf.Lerp(sizeNow[i], i == ready ? 1f : waitingSize, follow);
            float size = sizeNow[i];
            if (i == ready) size *= 1f + Mathf.Sin(Time.time * 6f) * readyPulse;
            d.localScale = new Vector3(worldScale.x * sx, worldScale.y, 1f) * size * (1f + pop[i]);

            // the ready familiar is tinted and pulses, the waiting ones are tinted differently
            SpriteRenderer sr = d.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.Lerp(sr.color, i == ready ? readyColor : waitingColor, follow);
        }
    }

    void OnDestroy()
    {
        if (ring != null) Destroy(ring.gameObject);
        foreach (Transform dock in docks)
            if (dock != null) Destroy(dock.gameObject);
        if (aimRenderer != null) aimRenderer.enabled = true;
    }

    private void UpdateRing(int ready)
    {
        if (ring == null) return;
        bool on = showGlowRing && ready >= 0 && docks[ready] != null;
        ring.gameObject.SetActive(on);
        if (!on) return;

        ring.position = docks[ready].position;
        float wave = 0.5f + 0.5f * Mathf.Sin(Time.time * 6f);   // 0 = lowest point, 1 = peak
        float pulse = 1f + wave * 0.06f;
        SpriteRenderer dockSprite = docks[ready].GetComponent<SpriteRenderer>();
        float baseSize = dockSprite != null ? Mathf.Max(dockSprite.bounds.size.x, dockSprite.bounds.size.y) : worldScale.y;
        float diameter = baseSize * ringSize * pulse;
        ring.localScale = new Vector3(diameter, diameter, 1f);
        ringRenderer.color = Color.Lerp(ringColorLow, ringColorPeak, wave);
    }

    // Sends the ready familiar out as a projectile. Returns false if none are left.
    public bool TryFire(GameObject projectilePrefab, Vector3 position, Quaternion rotation)
    {
        int slot = ReadySlot();
        if (slot < 0) return false;

        Vector3 from = docks[slot] != null ? docks[slot].position : position;
        slotAway[slot] = true;
        Available--;

        GameObject shot = Instantiate(projectilePrefab, from, rotation);
        projectile p = shot.GetComponent<projectile>();
        if (p != null) p.SetLauncher(this, slot);

        // The familiar is the projectile: hide the spell art and show the familiar instead
        foreach (SpriteRenderer sr in shot.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
        GameObject body = CreateVisual("FamiliarBody", shot.transform);
        body.transform.localPosition = Vector3.zero;
        return true;
    }

    // Called by a familiar projectile when it gets back to the witch (or is lost)
    public void FamiliarReturned(int slot)
    {
        if (slot >= 0 && slot < slotAway.Length && slotAway[slot])
        {
            slotAway[slot] = false;
            Available++;
            if (slot < docks.Count && docks[slot] != null)
            {
                docks[slot].position = transform.position;
                pop[slot] = 0.6f;
            }
        }
    }

    // Impact juice: sparks, a small camera shake
    public void PlayHitFeedback(Vector3 position, bool hitSomethingAlive)
    {
        HitSpark.Spawn(position, hitSomethingAlive ? new Color(1f, 0.55f, 0.35f) : new Color(1f, 0.9f, 0.6f), hitSomethingAlive ? 16 : 10);
        if (controller != null) controller.StartCoroutine(controller.createScreenShake(hitSomethingAlive ? 4 : 2));
    }
}
