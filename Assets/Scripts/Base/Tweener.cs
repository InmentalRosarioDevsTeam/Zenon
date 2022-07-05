using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class Tweener : MonoBehaviour
{
	private YieldInstruction tweenYieldInstruction = new WaitForEndOfFrame();
	static private Tweener _instance;

	static public Tweener instance
	{
		get
		{
			if ( _instance == null )
			{
				GameObject go = GameObject.Find("Tweener");
				if ( go == null )
				{
					go = new GameObject();
					go.name = "Tweener";
				}

				_instance = go.GetComponent<Tweener>() ?? go.AddComponent<Tweener>() ;
			}

			return _instance;
		}
	}

	public class TweenParameters
	{
		static readonly internal Vector3 DEFAULT_V3 = new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
		static readonly internal Vector2 DEFAULT_V2 = new Vector2 (Mathf.Infinity, Mathf.Infinity);
		static readonly internal Quaternion DEFAULT_QUATERNION = new Quaternion (Mathf.Infinity, Mathf.Infinity,Mathf.Infinity, Mathf.Infinity);

		public bool from = false;
		public Action<object> onComplete = null;
		public object onCompleteParameter;
		public Vector3 localScale = DEFAULT_V3;
		public Vector2 anchoredPosition = DEFAULT_V2;
		public Quaternion rotation = DEFAULT_QUATERNION;
		public Ease ease = Ease.None;
        public float layout_element_prefered_height = float.PositiveInfinity;
        public float delay = 0;
        internal Vector2 sizeDelta = DEFAULT_V2;
        internal float image_fillAmount = float.PositiveInfinity;
        internal Vector2 bezier = DEFAULT_V2;
        internal Vector3 position = DEFAULT_V3;
    }

	public Coroutine Tween( GameObject gameObject , float time , TweenParameters parameters )
	{
		return StartCoroutine (Tween_Coroutine(gameObject, time, parameters));
	}

	public void KillTween( Coroutine tween )
	{
		StopCoroutine (tween);
	}

    public void KillAllTweens()
    {
        StopAllCoroutines();
    }

    private IEnumerator Tween_Coroutine ( GameObject gameObject , float time , TweenParameters parameters )
	{

		float elapsedTime = 0.0f;
		float prc = 0.0f;

		RectTransform rectTransform = gameObject.GetComponent<RectTransform> ();
        LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
        Image image = gameObject.GetComponent<Image>();

        Vector3 ilocalScale = gameObject.transform.localScale;
        Vector3 iPosition = gameObject.transform.position;
		Vector2 ianchoredPosition = (rectTransform!=null)?rectTransform.anchoredPosition:default(Vector2);
        Vector2 iSizeDelta = (rectTransform != null) ? rectTransform.sizeDelta : default(Vector2);
        Quaternion iRotation = gameObject.transform.rotation;
        float i_layout_element_prefered_height = (layoutElement != null) ? layoutElement.preferredHeight : float.PositiveInfinity;
        float i_image_fillAmount = (image != null) ? image.fillAmount : float.PositiveInfinity;

        Type t = typeof(Tweener);
		System.Reflection.MethodInfo method = t.GetMethod (parameters.ease.ToString ());

		if (parameters.from) 
		{
			if(!parameters.localScale.Equals(TweenParameters.DEFAULT_V3)) gameObject.transform.localScale = parameters.localScale;
			if(!parameters.position.Equals(TweenParameters.DEFAULT_V3)) gameObject.transform.position = parameters.position;

			if (rectTransform != null) 
			{
				if(!parameters.anchoredPosition.Equals(TweenParameters.DEFAULT_V2))
					rectTransform.anchoredPosition = parameters.anchoredPosition;

                if (!parameters.sizeDelta.Equals(TweenParameters.DEFAULT_V2))
                    rectTransform.sizeDelta = parameters.sizeDelta;
            }

			if(!parameters.rotation.Equals(TweenParameters.DEFAULT_QUATERNION)) gameObject.transform.rotation = parameters.rotation;

            if (layoutElement != null)
            {
                if (parameters.layout_element_prefered_height != float.PositiveInfinity)
                    layoutElement.preferredHeight = parameters.layout_element_prefered_height;
            }

            if (image != null)
            {
                if (parameters.image_fillAmount != float.PositiveInfinity)
                    image.fillAmount = parameters.image_fillAmount;
            }
        }

        if (parameters.delay > 0)
            yield return new WaitForSeconds(parameters.delay);

        while (elapsedTime < time)
		{
			yield return tweenYieldInstruction;

            if (gameObject == null)
                yield return null;

			elapsedTime += Time.deltaTime;
			prc = Mathf.Clamp(elapsedTime / time,0,1);

			if (parameters.from)
				prc = 1 - prc;

			prc = (float)method.Invoke (this, new object[4]{ prc, 0, 1f, 1f}) ;

			if(!parameters.localScale.Equals(TweenParameters.DEFAULT_V3))
				gameObject.transform.localScale = ( parameters.localScale - ilocalScale) * prc + ilocalScale;

            if (!parameters.position.Equals(TweenParameters.DEFAULT_V3))
                gameObject.transform.position = (parameters.position - iPosition) * prc + iPosition;

            if (rectTransform != null) 
			{
				if (!parameters.anchoredPosition.Equals(TweenParameters.DEFAULT_V2))
				{
                    if (parameters.bezier.Equals(TweenParameters.DEFAULT_V2))
                        rectTransform.anchoredPosition = (parameters.anchoredPosition - ianchoredPosition) * prc + ianchoredPosition;
                    else
                        rectTransform.anchoredPosition = Bezier.Quadratic(prc, ianchoredPosition, parameters.bezier, parameters.anchoredPosition);
				}

                if (!parameters.sizeDelta.Equals(TweenParameters.DEFAULT_V2))
                {
                    rectTransform.sizeDelta = (parameters.sizeDelta - iSizeDelta) * prc + iSizeDelta;
                }
            }

			if (!parameters.rotation.Equals(TweenParameters.DEFAULT_QUATERNION)) {
				gameObject.transform.rotation = Quaternion.Lerp (iRotation,parameters.rotation, prc);
			}

            if (layoutElement != null)
            {
                if (parameters.layout_element_prefered_height != float.PositiveInfinity)
                {
                    layoutElement.preferredHeight = (parameters.layout_element_prefered_height - i_layout_element_prefered_height) * prc + i_layout_element_prefered_height;
                }
            }

            if (image != null)
            {
                if (parameters.image_fillAmount != float.PositiveInfinity)
                {
                    image.fillAmount = (parameters.image_fillAmount - i_image_fillAmount) * prc + i_image_fillAmount;
                }
            }

        }

		if (parameters.onComplete != null) 
		{
			parameters.onComplete(parameters.onCompleteParameter);
		}
	}

	#region Ease Functions

	public enum Ease
	{
		None,
		EaseInBack,
		EaseOutBack,
		EaseInOutBack,
	};

	/*
	None
	---------------------------------------------------------------------------------
	*/

	public static float None(float t, float b , float c, float d)
	{
		return t / d * (c-b) + b;
	}

	/*
	Back
	---------------------------------------------------------------------------------
	*/

	public static float EaseInBack (float t, float b , float c, float d)
	{
		float s = 1.70158f;
		return c*(t/=d)*t*((s+1)*t - s) + b;
	}
	public static float EaseOutBack (float t, float b , float c, float d)
	{
		float s = 1.70158f;
		return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
	}
	public static float EaseInOutBack (float t, float b , float c, float d)
	{
		float s = 1.70158f;
		if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525f))+1)*t - s)) + b;
		return c/2*((t-=2)*t*(((s*=(1.525f))+1)*t + s) + 2) + b;
	}

	#endregion
}

