using System.Collections.Generic;
using UnityEngine;

// Da mettere sulla radice del template della stanza tesoro. I piedistalli sono
// i punti in cui compaiono i potenziamenti offerti: uno per piedistallo, tutti
// diversi fra loro. Il player ne prende uno solo, gli altri spariscono.
public class TreasureRoom : MonoBehaviour
{
    [SerializeField] private Transform[] pedestals;

    // I potenziamenti creati da Populate: servono per poterli togliere di mezzo
    // quando il player sceglie
    private readonly List<GameObject> offers = new List<GameObject>();

    // Chiamato dal DungeonGenerator subito dopo aver istanziato la stanza. La
    // stanza a quel punto è ancora disattivata: i potenziamenti nascono figli
    // suoi e quindi disattivati anche loro, si accenderanno con la stanza.
    public void Populate(GameObject[] upgradePrefabs)
    {
        if (pedestals == null || pedestals.Length == 0 || upgradePrefabs == null) return;

        // Copia mescolata dei prefab: si pescano in ordine senza ripetizioni,
        // così i potenziamenti offerti sono sempre diversi fra loro
        List<GameObject> pool = new List<GameObject>();

        foreach (GameObject prefab in upgradePrefabs)
        {
            if (prefab != null) pool.Add(prefab);
        }

        Shuffle(pool);

        offers.Clear();

        // Se i potenziamenti sono meno dei piedistalli, qualche piedistallo
        // resta vuoto: meglio che offrire due volte la stessa cosa
        int amount = Mathf.Min(pedestals.Length, pool.Count);

        for (int i = 0; i < amount; i++)
        {
            Transform pedestal = pedestals[i];
            if (pedestal == null) continue;

            offers.Add(Instantiate(pool[i], pedestal.position, Quaternion.identity, transform));
        }
    }

    // Chiamato dal potenziamento raccolto, prima che si distrugga da solo
    public void OnUpgradeTaken(GameObject taken)
    {
        foreach (GameObject offer in offers)
        {
            if (offer == null || offer == taken) continue;

            Destroy(offer);
        }

        offers.Clear();
    }

    // Fisher-Yates
    private static void Shuffle(List<GameObject> items)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            GameObject swap = items[i];
            items[i] = items[j];
            items[j] = swap;
        }
    }
}
