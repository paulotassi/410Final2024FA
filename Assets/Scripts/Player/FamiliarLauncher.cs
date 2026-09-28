using System.Collections.Generic;
using UnityEngine;

// Ammo for the witch's normal shots: three familiars orbit her. Each shot sends one out as the projectile;
// after hitting something (or reaching max range) it flies back and is ready to be sent out again.
public class FamiliarLauncher : MonoBehaviour
{
    [Header("Familiars")]
    public int maxFamiliars = 3;
    public float orbitRadius = 1.4f;
    public float orbitSpeed = 90f;      // degrees per second
    public float dockScale = 0.6f;

    [Header("Flight")]
    [Tooltip("Delay between two shots. Keep it short: the number of familiars is the real limit")]
    public float fireInterval = 0.15f;
    [Tooltip("How far a familiar flies before it turns around")]
    public float maxRange = 14f;
    [Tooltip("Return speed as a multiple of the outbound speed. Lower = slower reload")]
    public float returnSpeedMultiplier = 0.4f;

    private PlayerController controller;
    private GameObject visualTemplate;
    private readonly List<Transform> docks = new List<Transform>();
    private bool[] slotAway;
    private float angle;

    public int Available { get; private set; }
    public bool HasAmmo => Available > 0;

    // Rapid Fire upgrades speed the familiars up (and so shorten the reload)
    public float SpeedMultiplier => controller != null ? controller.familiarSpeedMultiplier : 1f;

    public void Init(PlayerController owner, GameObject familiarVisual)
    {
        controller = owner;
        visualTemplate = familiarVisual;
        slotAway = new bool[maxFamiliars];
        Available = maxFamiliars;

        for (int i = 0; i < maxFamiliars; i++)
        {
            Transform dock = CreateVisual("FamiliarDock" + i, null).transform;
            docks.Add(dock);
        }
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
        visual.transform.localScale = Vector3.one * dockScale;
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.sortingOrder += 5;
        return visual;
    }

    void Update()
    {
        angle += orbitSpeed * Time.deltaTime;
        for (int i = 0; i < docks.Count; i++)
        {
            if (docks[i] == null) continue;
            float a = (angle + i * 360f / maxFamiliars) * Mathf.Deg2Rad;
            docks[i].position = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * orbitRadius;
            docks[i].gameObject.SetActive(!slotAway[i]);
        }
    }

    void OnDestroy()
    {
        foreach (Transform dock in docks)
            if (dock != null) Destroy(dock.gameObject);
    }

    // Sends one familiar out as a projectile. Returns false if none are docked.
    public bool TryFire(GameObject projectilePrefab, Vector3 position, Quaternion rotation)
    {
        if (Available <= 0) return false;

        int slot = System.Array.IndexOf(slotAway, false);
        slotAway[slot] = true;
        Available--;

        GameObject shot = Instantiate(projectilePrefab, position, rotation);
        projectile p = shot.GetComponent<projectile>();
        if (p != null) p.SetLauncher(this, slot);

        // The familiar is the projectile: hide the spell art and show the familiar instead
        foreach (SpriteRenderer sr in shot.GetComponentsInChildren<SpriteRenderer>()) sr.enabled = false;
        CreateVisual("FamiliarBody", shot.transform).transform.localPosition = Vector3.zero;
        return true;
    }

    // Called by a familiar projectile when it gets back to the witch (or is lost)
    public void FamiliarReturned(int slot)
    {
        if (slot >= 0 && slot < slotAway.Length && slotAway[slot])
        {
            slotAway[slot] = false;
            Available++;
        }
    }
}
