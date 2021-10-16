using System;
using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class TwoStateToggle<T> : VisualElement where T : struct
{
    public T state;
    public int index;

    public TwoStateToggle(T defaultState, Action<TwoStateToggle<T>> clickEvent, List<Color> colors, List<Texture2D> icons)
    {
        state = defaultState;
        style.flexDirection = FlexDirection.Row;
        style.alignItems = Align.Center;
        // style.left = -16;
        T[] Arr = (T[])Enum.GetValues(state.GetType());
        index = Array.IndexOf(Arr, state);
        float amount = Arr.Length;
        var box = new Box
        {
            pickingMode = PickingMode.Ignore,
            
            style =
            {
                borderBottomLeftRadius = 4,
                borderBottomRightRadius = 4,
                borderTopLeftRadius = 4,
                borderTopRightRadius = 4,

                borderBottomWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderTopWidth = 1,

                borderBottomColor = new Color(0.34f, 0.34f, 0.34f),
                borderLeftColor = new Color(0.34f, 0.34f, 0.34f),
                borderRightColor = new Color(0.34f, 0.34f, 0.34f),
                borderTopColor = new Color(0.34f, 0.34f, 0.34f),
                
                // backgroundColor = colors[index],

                
                height = 16,
                width = 20 * Enum.GetValues(typeof(T)).Length
            }
        };

        

        var button = new Button
        {
            style =
            {
                left = -20.1f * (amount-index),
                marginLeft = 0,
                marginRight = -2,
                height = 20,
                width = 24,

                unityBackgroundScaleMode = ScaleMode.ScaleToFit,
                // backgroundImage = icons[index],
                
                borderBottomLeftRadius = 6,
                borderBottomRightRadius = 6,
                borderTopLeftRadius = 6,
                borderTopRightRadius = 6,
            }
        };

        var t = button as ITransitionAnimations;
        button.clickable.clicked += () =>
        {
            state = state.Next();
            T[] Arr = (T[])Enum.GetValues(state.GetType());
            index = Array.IndexOf(Arr, state);
            t.Start(element => element.style.left.value.value, -20.1f * (amount-index), 100, (element, f) => button.style.left = f);
            t.Start(element => box.style.backgroundColor.value, colors[index], 350, (element, color1) => box.style.backgroundColor = color1);
            button.style.backgroundImage = icons[index];
            clickEvent(this);
        };

        Add(box);
        Add(button);


        //     clickable.clicked += () =>
        //     {
        //         state = state.Next();
        //         var t = this as ITransitionAnimations;
        //         
        //         clickEvent(this);
        //     };
    }
}

public static class Extensions
{
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] Arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(Arr, src) + 1;
        return (Arr.Length == j) ? Arr[0] : Arr[j];
    }
}