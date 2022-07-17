﻿using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System.Collections;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine.UI;
using SimpleMan.CoroutineExtensions;

namespace Sperlich.Extensions {
	public static class ExtensionMethods {

		/// <summary>
		/// Helpers to run Coroutines
		/// </summary>
		private static ExtensionMethodHelper _helperInstance;
		public static ExtensionMethodHelper Helper {
			get {
				if (_helperInstance == null) {
					_helperInstance = new GameObject("ExtensionMethods_Helper").AddComponent<ExtensionMethodHelper>();
				}
				return _helperInstance;
			}
		}

		public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax) {
			var fromAbs = from - fromMin;
			var fromMaxAbs = fromMax - fromMin;

			var normal = fromAbs / fromMaxAbs;

			var toMaxAbs = toMax - toMin;
			var toAbs = toMaxAbs * normal;

			var to = toAbs + toMin;

			return to;
		}
		public static float Remap(this int from, int fromMin, int fromMax, float toMin, float toMax) {
			var fromAbs = from - fromMin;
			var fromMaxAbs = fromMax - fromMin;

			var normal = fromAbs / fromMaxAbs;

			var toMaxAbs = toMax - toMin;
			var toAbs = toMaxAbs * normal;

			var to = toAbs + toMin;

			return to;
		}
		public static int ExtractLayer(this LayerMask layerMask) {
			int layerNumber = 0;
			int layer = layerMask.value;
			while (layer > 0) {
				layer = layer >> 1;
				layerNumber++;
			}
			return layerNumber - 1;
		}

		public static int Layer(this RaycastHit hit) => hit.transform.gameObject.layer;


		/// <summary>
		/// Searches for a component on the given Transform. Search order goes by: Transform itself -> Transforms Children -> Transforms Parent
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="t"></param>
		/// <returns></returns>
		public static T SearchComponent<T>(this Transform t) {
			T comp = t.GetComponent<T>();
			if (comp == null) {
				comp = t.GetComponentInChildren<T>();
			}
			if (comp == null) {
				comp = t.GetComponentInParent<T>();
			}
			return comp;
		}
		public static bool TrySearchComponent<T>(this Transform t, out T component) {
			component = t.SearchComponent<T>();
			if (component == null) {
				return false;
			}
			return true;
		}
		public static GameObject FindChild(this MonoBehaviour mono, string name) => mono.gameObject.FindChild(name);
		public static GameObject FindChild(this Transform transform, string name) => transform.gameObject.FindChild(name);
		public static GameObject FindChild(this GameObject gameobject, string name) {
			name = name.ToLower();
			GameObject result = null;
			foreach (Transform t in gameobject.transform) {
				if (t.name.ToLower() == name) {
					result = t.gameObject;
				}
			}
			if (result == null) {
				UnityEngine.Debug.LogWarning($"Unable to find {name}");
			}
			return result;
		}
		public static void DestroyAllChildren(this Transform transform, bool immediate = false) {
			foreach (Transform t in transform) {
				if (immediate) {
					UnityEngine.Object.DestroyImmediate(t.gameObject);
				} else {
					UnityEngine.Object.Destroy(t.gameObject);
				}
			}
		}
		public static T PickRandom<T>(this IEnumerable<T> source) {
			return source.PickRandom(1).Single();
		}

		public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count) {
			return source.Shuffle().Take(count);
		}

		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) {
			return source.OrderBy(x => Guid.NewGuid());
		}
		public static void Show(this MonoBehaviour mono) {
			mono.gameObject.SetActive(true);
		}
		public static void Hide(this MonoBehaviour mono) {
			mono.gameObject.SetActive(false);
		}
		public static void Show(this GameObject gameobject) {
			gameobject.gameObject.SetActive(true);
		}
		public static void Hide(this GameObject gameobject) {
			gameobject.gameObject.SetActive(false);
		}
		public static void Show(this Transform transform) {
			transform.gameObject.SetActive(true);
		}
		public static void Hide(this Transform transform) {
			transform.gameObject.SetActive(false);
		}
		public static void Stretch(this MonoBehaviour t, float original, float to, float duration, Ease ease = Ease.Linear) => Stretch(t.gameObject, original, to, duration, ease);
		public static void Stretch(this GameObject t, float original, float to, float duration, Ease ease = Ease.Linear) {
			Sequence seq = DOTween.Sequence();
			seq.Append(t.transform.DOScale(to, duration / 2f)).SetEase(ease);
			seq.Append(t.transform.DOScale(original, duration / 2f)).SetEase(ease);
			seq.Play();
		}
		public static RectTransform RectTransform(this Component t) {
			return t.GetComponent<RectTransform>();
		}
		public static void CountUp(this TMP_Text text, int from, int to, float duration) {
			Helper.StartCoroutine(ICount());
			IEnumerator ICount() {
				float time = 0;
				float stretchTime = 0;
				while (time < duration) {
					int current = (int)Mathf.Lerp(from, to, time.Remap(0, duration, 0, 1));
					text.SetText(current.ToString());
					if (stretchTime > duration / 20f) {
						stretchTime = 0;
						text.Stretch(1f, 1.3f, duration / 20f);
					}
					yield return null;
					time += Time.deltaTime;
					stretchTime += Time.deltaTime;
				}
			}
		}
		public static void TextCountUp(this Text text, int from, int to, float duration) {
			Helper.StartCoroutine(ICount());
			IEnumerator ICount() {
				float time = 0;
				float stretchTime = 0;
				while (time < duration) {
					int current = (int)Mathf.Lerp(from, to, time.Remap(0, duration, 0, 1));
					text.text = current.ToString();
					if (stretchTime > duration / 20f) {
						stretchTime = 0;
						text.Stretch(1f, 1.3f, duration / 20f);
					}
					yield return null;
					time += Time.deltaTime;
					stretchTime += Time.deltaTime;
				}
			}
		}
		public static object CloneObject(this object objSource) {
			//Get the type of source object and create a new instance of that type
			Type typeSource = objSource.GetType();
			object objTarget = Activator.CreateInstance(typeSource);
			//Get all the properties of source object type
			PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			//Assign all source property to taget object 's properties
			foreach (PropertyInfo property in propertyInfo) {
				//Check whether property can be written to
				if (property.CanWrite) {
					//check whether property type is value type, enum or string type
					if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String))) {
						property.SetValue(objTarget, property.GetValue(objSource, null), null);
					}
					//else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
					else {
						object objPropertyValue = property.GetValue(objSource, null);
						if (objPropertyValue == null) {
							property.SetValue(objTarget, null, null);
						} else {
							property.SetValue(objTarget, objPropertyValue.CloneObject(), null);
						}
					}
				}
			}
			return objTarget;
		}
		public static Vector2 Vector2XZ(this Vector3 vec) => new Vector2(vec.x, vec.z);
		public static GameObject Spawn(this GameObject _obj, Vector3 position, float deleteAfter = 0) {
			var o = UnityEngine.Object.Instantiate(_obj, position, Quaternion.identity);

			if(deleteAfter > 0) {
				Helper.Delay(deleteAfter, () => {
					UnityEngine.Object.Destroy(o);
				});
			}
			return o;
		}

		// Helper for Coroutines
		public sealed class ExtensionMethodHelper : MonoBehaviour { }
	}
}