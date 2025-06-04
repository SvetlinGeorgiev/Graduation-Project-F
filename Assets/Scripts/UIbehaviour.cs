using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

public class UIbehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayableDirector Sequencer;
    void Awake()
    {
        Debug.Log("Awake");
        
        
    }
    void Start()
    {
        Debug.Log("Start");

        Sequencer.Stop();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Play Enter");
        Sequencer.Stop();



    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Play Exit");
        Sequencer.Stop();
       
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    

    // Update is called once per frame
    void Update()
    {
        
    }


    
    //private void OnMouseEnter()
    //{
    //    Debug.Log("Play");
    //    Sequencer.Play();
    //}
    //private void OnMouseLeave()
    //{
    //    Sequencer.Stop();
    //}

   
}
