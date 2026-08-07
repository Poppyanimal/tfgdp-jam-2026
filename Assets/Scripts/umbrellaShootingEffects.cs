using UnityEngine;

public class umbrellaShootingEffects : MonoBehaviour
{
    public ParticleSystem hand_swivels, energy_ready, energy_out, exhaust_steam, exhaust_cartridge;
    
    public void playHandEffects() { hand_swivels.Play(); }
    public void playEnergyReadyEffects() { energy_ready.Play(); }
    public void playEnergyOutEffects() { energy_out.Play(); }
    public void playExhaustSteamEffects() { exhaust_steam.Play(); }
    public void playCartridgeEffects() { exhaust_cartridge.Play(); }
}
