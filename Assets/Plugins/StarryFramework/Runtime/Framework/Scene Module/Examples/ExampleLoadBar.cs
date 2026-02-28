using StarryFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarryFramework
{
    public class ExampleLoadBar : LoadProgressBase
    {
        [SerializeField]
        protected Slider progressBar;
        [SerializeField]
        protected Text _text;
        [SerializeField]
        [Range(0, 4)]
        protected int decimalPlaces = 2;

        public override void SetProgressValue(float value)
        {
            progressBar.value = value;
            _text.text = Math.Round(value * 100, decimalPlaces).ToString() + "%";
        }

        public override void BeforeSetActive(AsyncOperation asyncOperation)
        {

            _text.text = "Press any key to start.";
            StartCoroutine(Routine());
            return;

            IEnumerator Routine()
            {
                while(true)
                {
                    if (Input.anyKeyDown)
                    {
                        AllowSceneActivate(asyncOperation);
                        break;
                    }

                    yield return null;
                }
            }
            
        }
    }
}
