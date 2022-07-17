using System.Collections;
using System.Collections.Generic;
using CarterGames.Assets.AudioManager;
using UnityEngine;

public class AudioPlayer {

	public static AudioManager Player => AudioManager.instance;

	public static void Play(string track, float randomMinPitch = 1f, float randomMaxPitch = 1f, float volume = 1f) {
		if(Player.HasClip(track)) {
			Player.Play(track, 0, volume, Random.Range(randomMinPitch, randomMaxPitch));
		} else {
			Debug.LogError($"Error: Trying to play {track}, but couldnt be found.");
		}
	}

	public static AudioReverbFilter Play(string track, AudioReverbPreset preset, float randomMinPitch = 1f, float randomMaxPitch = 1f, float volume = 1f) {
		if (Player.HasClip(track)) {
			AudioSource source = Player.PlayAndGetSource(track, 0, volume, Random.Range(randomMinPitch, randomMaxPitch));
			var reverbFilter = source.gameObject.AddComponent<AudioReverbFilter>();
			reverbFilter.reverbPreset = preset;
			return reverbFilter;
		} else {
			Debug.LogError($"Error: Trying to play {track}, but couldnt be found.");
			return null;
		}
	}
}
