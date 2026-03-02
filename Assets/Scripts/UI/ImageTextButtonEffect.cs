using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ImageTextButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("图片文字组件")]
    [SerializeField] private Image textImage;

    [Header("悬停效果参数")]
    public Color hoverColor = new Color(1f, 1f, 1f, 1f);
    public float hoverScale = 1.1f;
    public float animDuration = 0.15f;

    private Color _originalColor;
    private Vector3 _originalScale;
    private Button _button;
    private bool _isHovered;

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (textImage == null)
        {
            textImage = GetComponentInChildren<Image>(true);
            if (textImage != null && textImage.name != "TextImage")
            {
                textImage = transform.Find("TextImage")?.GetComponent<Image>();
            }
        }

        if (textImage == null)
        {
            Debug.LogError("未找到TextImage组件！", this);
            enabled = false;
            return;
        }

        _originalColor = textImage.color;
        _originalScale = textImage.rectTransform.localScale;
        _button.interactable = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        _isHovered = true;
        AnimateToHoverState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;
        AnimateToOriginalState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;
        StopAllCoroutines();
        textImage.color = _originalColor;
        textImage.rectTransform.localScale = _originalScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isHovered && _button.interactable)
        {
            AnimateToHoverState();
        }
    }

    private void AnimateToHoverState()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateImage(_originalColor, hoverColor, _originalScale, _originalScale * hoverScale));
    }

    private void AnimateToOriginalState()
    {
        StopAllCoroutines();
        StartCoroutine(AnimateImage(textImage.color, _originalColor, textImage.rectTransform.localScale, _originalScale));
    }

    private IEnumerator AnimateImage(Color startColor, Color targetColor, Vector3 startScale, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        while (elapsedTime < animDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / animDuration);

            textImage.color = new Color(
                Mathf.Lerp(startColor.r, targetColor.r, t),
                Mathf.Lerp(startColor.g, targetColor.g, t),
                Mathf.Lerp(startColor.b, targetColor.b, t),
                Mathf.Lerp(startColor.a, targetColor.a, t)
            );

            textImage.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);

            yield return null;
        }

        textImage.color = targetColor;
        textImage.rectTransform.localScale = targetScale;
    }
}