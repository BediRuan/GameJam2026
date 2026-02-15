////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Martin Bustos @FronkonGames <fronkongames@gmail.com>. All rights reserved.
//
// THIS FILE CAN NOT BE HOSTED IN PUBLIC REPOSITORIES.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;

namespace FronkonGames.SpiceUp.Shake
{
  ///------------------------------------------------------------------------------------------------------------------
  /// <summary> Settings. </summary>
  /// <remarks> Only available for Universal Render Pipeline. </remarks>
  ///------------------------------------------------------------------------------------------------------------------
  public sealed partial class Shake
  {
    private static GameObject shakeObject;
    private static ShakeManager shakeManager;

    /// <summary> One shake. </summary>
    public static void Start()
    {
      GetShakeManager().Start();
    }

    /// <summary> Stop shake. </summary>
    public static void Stop() => shakeManager?.Stop();

    private static ShakeManager GetShakeManager()
    {
      if (shakeManager == null)
      {
        if (shakeObject == null)
        {
          shakeObject = GameObject.Find("Shake Manager");
          if (shakeObject == null)
            shakeObject = new GameObject("Shake Manager") { hideFlags = HideFlags.HideAndDontSave };
        }

        shakeManager = shakeObject.GetComponent<ShakeManager>();
        if (shakeManager == null)
          shakeManager = shakeObject.AddComponent<ShakeManager>();
      }

      return shakeManager;
    }

    /// <summary> Internal use. </summary>
    private class ShakeManager : MonoBehaviour
    {
      private Settings settings;

      private Coroutine coroutine;

      public void Start()
      {
        if (coroutine != null)
          StopCoroutine(coroutine);

        coroutine = StartCoroutine(UpdatePulse());
      }

      public void Stop()
      {
        if (settings != null)
          settings.progress = 0.0f;

        if (coroutine != null)
          StopCoroutine(coroutine);

        coroutine = null;
      }

      private IEnumerator UpdatePulse()
      {
        float time = 0.0f;

        settings ??= Instance.settings;
        settings.progress = 0.0f;

        while (time < settings.duration)
        {
          settings.progress = time / settings.duration;
          time += Time.deltaTime;

          yield return null;
        }

        settings.progress = 0.0f;
        coroutine = null;
      }

      private void OnEnable() => Stop();

      private void OnDisable() => Stop();
    }
  }
}
