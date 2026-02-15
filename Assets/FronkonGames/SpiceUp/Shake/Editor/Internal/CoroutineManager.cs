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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace FronkonGames.SpiceUp.Shake
{
  /// <summary> Coroutines for the Editor. Internal Use. </summary>
  [InitializeOnLoad]
  internal static class CoroutineManager
  {
    private static readonly List<IEnumerator> CoroutineInProgress = new();
    private static int currentExecute = 0;
    private static float timeSince = 0.0f;

    private static EditorWindow GameView;

    public static float DeltaTime()
    {
      if (timeSince == 0.0f)
        timeSince = (float)EditorApplication.timeSinceStartup;

      float timeDelta = (float)EditorApplication.timeSinceStartup - timeSince;

      timeSince = (float)EditorApplication.timeSinceStartup;

      return timeDelta;
    }

    public static IEnumerator Start(IEnumerator coroutine)
    {
      timeSince = 0.0f;

      CoroutineInProgress.Add(coroutine);

      return coroutine;
    }

    public static void Stop(IEnumerator coroutine)
    {
      if (CoroutineInProgress.Contains(coroutine) == true)
        CoroutineInProgress.Remove(coroutine);

      coroutine = null;
    }

    static CoroutineManager()
    {
      EditorApplication.update += ExecuteCoroutine;
    }

    private static void ExecuteCoroutine()
    {
      if (CoroutineInProgress.Count <= 0)
        return;

      currentExecute = (currentExecute + 1) % CoroutineInProgress.Count;

      RepaintGameView();

      if (CoroutineInProgress[currentExecute].MoveNext() == false)
        CoroutineInProgress.RemoveAt(currentExecute);
    }

    private static void RepaintGameView()
    {
      if (GameView == null)
      {
        System.Reflection.Assembly assembly = typeof(EditorWindow).Assembly;
        Type type = assembly.GetType("UnityEditor.GameView");
        GameView = EditorWindow.GetWindow(type);
      }

      GameView.Repaint();
    }
  }
}