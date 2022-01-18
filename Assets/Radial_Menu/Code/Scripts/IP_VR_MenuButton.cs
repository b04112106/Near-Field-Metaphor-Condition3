using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IndiePixel.VR
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]
    public class IP_VR_MenuButton : MonoBehaviour 
    {
        #region Variables
        [Header("Button Properties")]
        public int buttonID;
        public string buttonText;
        public Image buttonIcon;
        public Sprite normalImage;
        public Sprite hoverImage;

        [Header("Events")]
        public UnityEvent OnClick = new UnityEvent();

        private Animator animator;
        private Image currentImage;
        private Color original;
        #endregion


        #region Main Methods
    	// Use this for initialization
    	void Start () 
        {
            animator = GetComponent<Animator>();
            currentImage = GetComponent<Image>();
            if(currentImage && normalImage)
            {
                currentImage.sprite = normalImage;
            }
            original = currentImage.color;
    	}
        #endregion


        #region Custom Methods
        public void Hover(int anID)
        {
            if(currentImage)
            {
                if(anID == buttonID && hoverImage)
                {
                    currentImage.sprite = hoverImage;
                    
                    HandleAnimator(true);
                }
                else if(normalImage)
                {
                    currentImage.sprite = normalImage;
                    HandleAnimator(false);
                }
            }
        }

        public void Click(int anID)
        {
            if(buttonID == anID)
            {
                if(OnClick != null)
                {
                    OnClick.Invoke();
                    changeMode(buttonID);
                    
                }
            }
        }
        
        void HandleAnimator(bool aToggle)
        {
            if(animator)
            {
                animator.SetBool("hover", aToggle);
            }
        }

        private void changeMode(int ID)
        {
            int mode = this.gameObject.transform.parent.transform.parent.GetComponent<IP_VR_RadialMenu>().menuMode;
            if(mode == ID)
            {
                mode = -1;
                currentImage.color = original;
            }
            else
            {
                mode = ID;
                currentImage.color = Color.green;
            }
            this.gameObject.transform.parent.transform.parent.GetComponent<IP_VR_RadialMenu>().menuMode = mode;
        }
        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void Update()
        {
            // if not select, change to original color
            if(this.gameObject.transform.parent.transform.parent.GetComponent<IP_VR_RadialMenu>().menuMode != buttonID)
                currentImage.color = original;
        }
        #endregion
    }
}
