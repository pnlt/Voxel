using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Akila.FPSFramework.AudioManagement
{
    [AddComponentMenu("Akila/FPS Framework/Audio System/Audio Filter Area")]
    public class AudioFilterArea : MonoBehaviour
    {
        CharacterManager characterManager;

        private void OnTriggerStay(Collider collider)
        {
            if (collider.TryGetComponent<CharacterManager>(out CharacterManager characterManager))
            {
                this.characterManager = characterManager;
                OnAreaEntered(characterManager, FindObjectOfType<AudioFiltersManager>());
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            if (characterManager) OnAreaExited(characterManager, FindObjectOfType<AudioFiltersManager>());
            characterManager = null;
        }

        protected virtual void OnAreaEntered(CharacterManager characterManager, AudioFiltersManager audioFiltersManager)
        {

        }

        protected virtual void OnAreaExited(CharacterManager characterManager, AudioFiltersManager audioFiltersManager)
        {

        }
    }
}