using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour {
	private static AudioManager I;

	private Dictionary<string, AudioClip[]> _soundsDictionary = new Dictionary<string, AudioClip[]>();
	private Dictionary<string, float> _volumeDictionary = new Dictionary<string, float>();

	private static AudioSource _audioSource;

	[SerializeField]
	private AudioSource z_AmbientAudioSource;
	
	private static AudioSource _ambientAudioSource;

	private Stack<AudioSource> _AudioSourcePool = new Stack<AudioSource>();

	private GameObject AudioSourceParent;


	public float PitchVariance = .15f;

	[Range(0, 1)] public float SpatialBlend = .3f;

	private void Awake() {
		if (I && I != this) {
			Destroy(this.gameObject);
			return;
		}

		I = this;
		LoadSoundClips();
		_audioSource = GetComponent<AudioSource>();
		_ambientAudioSource = I.z_AmbientAudioSource;
		AudioListener.volume = .2f;
	}

	// private void OnEnable() {
	// 	DesynchedNetworkManager.OnBeforeClientChangeScene += SceneCleanup;
	// 	DesynchedNetworkManager.OnBeforeServerChangeScene += SceneCleanup;
	// }
	//
	// private void OnDisable() {
	// 	DesynchedNetworkManager.OnBeforeClientChangeScene -= SceneCleanup;
	// 	DesynchedNetworkManager.OnBeforeServerChangeScene -= SceneCleanup;
	// }
	//
	// private void SceneCleanup() {
	// 	_AudioSourcePool.Clear();
	// }

	private void LoadSoundClips() {
		_soundsDictionary["wind"] = Resources.LoadAll<AudioClip>("Wind");
		_soundsDictionary["frogHit"] = Resources.LoadAll<AudioClip>("FrogHit");
		_soundsDictionary["carSmall"] = Resources.LoadAll<AudioClip>("CarSmall");
		_soundsDictionary["carBig"] = Resources.LoadAll<AudioClip>("CarBig");
		_soundsDictionary["carSport"] = Resources.LoadAll<AudioClip>("carSport");
		_soundsDictionary["carIdleSmall"] = Resources.LoadAll<AudioClip>("CarIdleSmall");
		_soundsDictionary["tireScreech"] = Resources.LoadAll<AudioClip>("TireScreech");
		_soundsDictionary["successSmall"] = Resources.LoadAll<AudioClip>("SuccessSmall");
		_soundsDictionary["drumFail"] = Resources.LoadAll<AudioClip>("DrumFail");
		_soundsDictionary["drumGo"] = Resources.LoadAll<AudioClip>("DrumGo");


		_volumeDictionary["tireScreech"] = .15f;
		_volumeDictionary["carIdleSmall"] = .2f;
		_volumeDictionary["carBig"] = .9f;

	}


	public static void PlaySoundAtPosition(Vector2 position, string sound, float pitch = -1) {
		if (!I._soundsDictionary.TryGetValue(sound, out var clips)) {
			Debug.LogWarning($"Sound file {sound} not found!");
			return;
		}

		if (!I._volumeDictionary.TryGetValue(sound, out float vol)) {
			vol = 1;
		}

		PlayRandomSoundAtPosition(position, clips, vol, pitch);
	}

	public static void PlaySound(string sound) {
		if (!I._soundsDictionary.TryGetValue(sound, out var clips)) {
			Debug.LogWarning($"Sound file {sound} not found!");
			return;
		}

		if (!I._volumeDictionary.TryGetValue(sound, out float vol)) {
			vol = .5f;
		}

		PlayRandomSound(clips, vol);
	}

	private static void PlayRandomSoundAtPosition(Vector2 position, AudioClip[] clips, float vol, float pitch = -1) {
		var rnd = Random.Range(0, clips.Length);
		PlayClipAtPoint(clips[rnd], position, vol, pitch);
	}

	private static void PlayRandomSound(AudioClip[] clips, float vol, float pitch = 1) {
		var rnd = Random.Range(0, clips.Length);
		_audioSource.pitch = pitch;
		_audioSource.PlayOneShot(clips[rnd], vol);
	}

	private static AudioSource PlayClipAtPoint(AudioClip clip, Vector2 pos, float vol, float pitch = -1, bool loop = false) {
		AudioSource source = GetFromPool();
		
		source.volume = vol;
		if (pitch < 0) {
			pitch = Random.Range(1 - I.PitchVariance, 1 + I.PitchVariance);
		}
		source.pitch = pitch;
		source.spatialBlend = I.SpatialBlend;
		source.transform.position = pos;
		source.clip = clip;
		source.Play();
		I.StartCoroutine(I.ReturnToPool(source, clip.length));
		return source;
	}

	private static AudioSource GetFromPool() {
		AudioSource source;
		if (I._AudioSourcePool.Count == 0) {
			GameObject tempGO = new GameObject("TempAudioSource");
			source = tempGO.AddComponent<AudioSource>();
			if (!I.AudioSourceParent) {
				I.AudioSourceParent = new GameObject("AudioSourceParent");
			}

			tempGO.transform.SetParent(I.AudioSourceParent.transform);
			source.spread = _audioSource.spread;
			source.dopplerLevel = _audioSource.dopplerLevel;
			source.rolloffMode = _audioSource.rolloffMode;
			source.minDistance = _audioSource.minDistance;
			source.maxDistance = _audioSource.maxDistance;
		}
		else {
			source = I._AudioSourcePool.Pop();
			source.gameObject.SetActive(true);
		}

		return source;
	}


	public static AudioSource PlaySoundUntilCancelled(string sound, Vector2 pos, bool loop, out Action cancelAction) {
		var source = GetFromPool();
		cancelAction = () => {
			source.Stop();
			if (I != null) { //can be null if called on end of playsession
				I.StartCoroutine(I.ReturnToPoolNextFrame(source));
			}
		};
		
		
		if (!I._soundsDictionary.TryGetValue(sound, out var clips)) {
			Debug.LogWarning($"Sound file {sound} not found!");
			return null;
		}

		if (!I._volumeDictionary.TryGetValue(sound, out float vol)) {
			vol = .5f;
		}
		
		var rnd = Random.Range(0, clips.Length);
		source.clip = clips[rnd];
		source.volume = vol;
		source.loop = loop;
		
		source.Play();

		return source;
	}

	public static void PlayAmbientLoop(string sound) {
		if (!I._soundsDictionary.TryGetValue(sound, out var clips)) {
			Debug.LogWarning($"Sound file {sound} not found!");
			return;
		}

		if (!I._volumeDictionary.TryGetValue(sound, out float vol)) {
			vol = .5f;
		}

		_ambientAudioSource.clip = clips[0];
		_ambientAudioSource.loop = true;
		_ambientAudioSource.Play();
	}

	private IEnumerator ReturnToPool(AudioSource src, float delay) {
		yield return new WaitForSecondsRealtime(delay);

		ReturnToPool(src);
	}
	
	private IEnumerator ReturnToPoolNextFrame(AudioSource src) {
		yield return null;

		ReturnToPool(src);
	}
	
	public static void ReturnSourceToPool(AudioSource source) {
		I.ReturnToPool(source);
	}

	private void ReturnToPool(AudioSource src) {
		if (!src) return;
		src.Stop();
		src.clip = null;
		src.gameObject.SetActive(false);
		src.transform.SetParent(I.AudioSourceParent.transform);
		_AudioSourcePool.Push(src);
	}
}