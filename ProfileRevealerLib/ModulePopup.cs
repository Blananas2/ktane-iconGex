using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProfileRevealerLib {
	public class ModulePopup : MonoBehaviour {
		public RectTransform Canvas;
		public Text Text;
		public RectTransform BoxCollider;
		public SpriteRenderer Icon;

		public bool Visible => this.Canvas.gameObject.activeSelf;

		public float Delay { get; set; }
		public Transform Module { get; set; }
		public string ProfileName { get => this.profileName; set { this.profileName = value; this.SetText(); } }
		internal IList<string> bossStatus;

		internal string moduleName;
		private string profileName;
		internal IEnumerable<string> enabledProfiles;
		internal IEnumerable<string> disabledProfiles;
		internal IEnumerable<string> inactiveProfiles;
		private Coroutine coroutine;

		private class RepoEntry {
			// Token: 0x04000037 RID: 55
			public string ModuleID;

			// Token: 0x04000038 RID: 56
			public string FileName;

			// Token: 0x04000039 RID: 57
			public string Name;

			// Token: 0x0400003A RID: 58
			public string Symbol;
		}

		// Token: 0x02000008 RID: 8
		private class RepoResult {
			// Token: 0x0400003B RID: 59
			public List<RepoEntry> KtaneModules;
		}

		public void Start() {
			this.Canvas.gameObject.SetActive(false);

			this.SetText();

			var colliders = new List<Collider>();
			foreach (var collider in this.Module.GetComponentsInChildren<Collider>(true)) {
				if (!collider.enabled) {
					colliders.Add(collider);
					collider.enabled = true;
				}
			}

			var halfExtents = this.BoxCollider.transform.lossyScale;
			halfExtents.Scale(new Vector3(300, 150, 100));
			if (Physics.CheckBox(this.BoxCollider.position, halfExtents, this.BoxCollider.rotation)) {
				var transform = (RectTransform) this.Canvas.GetChild(0);
				transform.pivot = new Vector2(0, 1);
			}

			foreach (var collider in colliders) collider.enabled = false;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000032C4 File Offset: 0x000016C4
		private IEnumerator GetRepoAndIcon() {
			using (UnityWebRequest req = UnityWebRequest.Get("https://ktane.timwi.de/json/raw")) {
				yield return req.SendWebRequest();
				if (req.isNetworkError || req.isHttpError) {
					Debug.LogFormat("[Icon Gex] Error fetching repository data: {0}", new object[]
					{
				
				req.error
					});
					Debug.LogFormat("[Icon Gex] (This error is standard in the case of a lack of internet.)", new object[]
					{
				//KEEP THIS IN MIND FOR LATER
					});
				} else {
					try {
						List<RepoEntry> ktaneModules = JsonConvert.DeserializeObject<RepoResult>(req.downloadHandler.text).KtaneModules;
						if (ktaneModules != null) {
							List<RepoEntry> list = (from e in ktaneModules
															   where e.Symbol != null && this.BombInfo.GetModuleIDs().Contains(e.ModuleID)
															   select e).ToList<RepoEntry>();
							if (list.Count > 0) {
								this.Symbols = (from e in list
												select e.Symbol).Distinct<string>().ToList<string>();
								RepoEntry repoEntry = list.PickRandom<RepoEntry>();
								Debug.LogFormat("[Icon Gex] Selected {0}", new object[]
								{
							
							repoEntry.Name
								});
								this.SelectedModuleSymbol = repoEntry.Symbol;
								base.StartCoroutine(this.FetchIcon(repoEntry));
								this.UpdateNumCounter();
							} else {
								Debug.LogFormat("[Icon Gex] Could not find any applicable icons for bomb modules. This shouldn't happen, please contact the mod author.", new object[]
								{
							
								});
								Debug.LogFormat("[Icon Gex] (Bomb modules: {0}, Repo entries: {1})", new object[]
								{
							
							JsonConvert.SerializeObject(this.BombInfo.GetModuleIDs()),
							JsonConvert.SerializeObject(ktaneModules)
								});
							}
						} else {
							Debug.LogFormat("[Icon Gex] The entries property is null. This shouldn't happen, please contact the mod author.", new object[]
							{
						
							});
						}
					} catch (Exception ex) {
						Debug.LogFormat("[Icon Gex] Error parsing repository data: {0}", new object[]
						{
					
					ex.Message
						});
						Debug.LogFormat("[Icon Gex] (This shouldn't happen, please contact the mod author.)", new object[]
						{
					
						});
					}
				}
			}
			yield break;
		}

		// Token: 0x0600003B RID: 59 RVA: 0x000032E0 File Offset: 0x000016E0
		private IEnumerator FetchIcon(RepoEntry entry) {
			WWW www = new WWW(repoURL + "/Icons/" + Uri.EscapeDataString(entry.FileName ?? entry.Name) + ".png");
			yield return www;
			if (www.error != null) {
				Debug.LogFormat("[Icon Gex] Error fetching icon for {0}: {1}", new object[]
				{
			
			entry.Name,
			www.error
				});
				Debug.LogFormat("[Icon Gex] (This error is standard in the case of a module without an icon.)", new object[]
				{
			
				});
			}
			Texture2D tex = new Texture2D(1, 1);
			www.LoadImageIntoTexture(tex);
			www.Dispose();
			www = null;
			tex.filterMode = 0;
			this.SetIcon(tex);
			yield break;
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003304 File Offset: 0x00001704
		private void SetIcon(Texture2D texture) {
			this.IconObject.material.mainTexture = texture;
			for (int i = 0; i < numSquares * numSquares; i++) {
				int num = i % numSquares;
				int num2 = Mathf.FloorToInt((float) i / (float) numSquares);
				float num3 = 1f / (float) numSquares;
				float num4 = (float) num - (float) numSquares / 2f + 0.5f;
				float num5 = (float) num2 - (float) numSquares / 2f + 0.5f;
				GameObject gameObject = new GameObject("CoverSquare");
				GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, this.IconObject.transform);
				Object.Destroy(gameObject);
				MeshRenderer meshRenderer = gameObject2.AddComponent<MeshRenderer>();
				meshRenderer.material = this.CoverMaterial;
				float num6 = (Random.value <= 0.4f) ? 0f : 1f;
				float num7 = Random.Range(0f, 0.2f);
				meshRenderer.material.color = new Color(num7, num7, num7, num6);
				gameObject2.AddComponent<MeshFilter>().mesh = this.PlaneMesh;
				gameObject2.transform.localScale = new Vector3(num3, num3, num3);
				gameObject2.transform.localPosition = new Vector3(num4, 0.02f, num5);
			}
		}

		//FUCK

		private void SetText() {
			var enabledProfilesStr = Join(", ", this.enabledProfiles);
			var disabledProfilesStr = Join(", ", this.disabledProfiles);
			var inactiveProfilesStr = Join(", ", this.inactiveProfiles);

			var builder = new StringBuilder();
			if (this.moduleName != null) builder.Append($"<b>{this.moduleName}</b>{(this.bossStatus != null ? " " : "\n")}");
			if (this.bossStatus != null) builder.AppendLine($"<color=red>({string.Join(", ", this.bossStatus.ToArray())})</color>");
			if (this.ProfileName != null)
				builder.AppendLine($"Chosen from: <color=yellow>{this.ProfileName}</color>");
			if (enabledProfilesStr.Length > 0)
				builder.AppendLine($"Enabled by: <color=lime>{enabledProfilesStr}</color>");
			if (disabledProfilesStr.Length > 0)
				builder.AppendLine($"Disabled by: <color=red>{disabledProfilesStr}</color>");
			if (inactiveProfilesStr.Length > 0)
				builder.AppendLine($"Inactive vetos: <color=silver>{inactiveProfilesStr}</color>");
			if (builder.Length == 0) this.Text.text = "No profiles found.";
			else {
				builder.Remove(builder.Length - 1, 1);
				this.Text.text = builder.ToString();
			}
		}

		private static string Join<T>(string separator, IEnumerable<T> enumerable) {
			if (enumerable == null) return "";
			var enumerator = enumerable.GetEnumerator();
			if (!enumerator.MoveNext()) return "";
			var builder = new StringBuilder();
			builder.Append(enumerator.Current);
			while (enumerator.MoveNext()) {
				builder.Append(separator);
				builder.Append(enumerator.Current);
			}
			return builder.ToString();
		}

		public void ShowDelayed() {
			if (this.coroutine != null) this.StopCoroutine(this.coroutine);
			this.coroutine = this.StartCoroutine(this.DelayCoroutine());
		}

		public void Show() {
			if (this.coroutine != null) this.StopCoroutine(this.coroutine);
			this.coroutine = null;
			this.Canvas.gameObject.SetActive(true);
		}

		public void Hide() {
			if (this.coroutine != null) this.StopCoroutine(this.coroutine);
			this.coroutine = null;
			this.Canvas.gameObject.SetActive(false);
		}

		private IEnumerator DelayCoroutine() {
			if (this.Delay < 0 || float.IsInfinity(this.Delay) || float.IsNaN(this.Delay)) yield break;
			yield return new WaitForSeconds(this.Delay);
			this.Canvas.gameObject.SetActive(true);
		}
	}
}
