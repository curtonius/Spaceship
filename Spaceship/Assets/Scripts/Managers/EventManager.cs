using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoSingleton<EventManager>
{
    private Dictionary<string, Delegate> _events = new Dictionary<string, Delegate>();
    //To call a function and pass in data
    public delegate void EventCallback<T>(T data);
    //To call a function without passing in data
    public delegate void EventCallback();
    //To call a function without passing in data, and return a value back to wherever the function was Raised
    public delegate T EventCallBackWithReturn<T>();

    public override void OnInitialize()
    {
        base.OnInitialize();
        this.StayAlive = true;
    }

    public void RegisterEvent(string eventName)
    {
        if (this._events.ContainsKey(eventName) == false)
        {
            this._events.Add(eventName, null);
        }
    }

    public bool IsEventRegistered(string eventName)
    {
        return (this._events.ContainsKey(eventName) == true);
    }

    public void RemoveEvent(string eventName)
    {
        if (this.IsEventRegistered(eventName) == true)
        {
            this._events.Remove(eventName);
        }
    }

    //To create a listener that takes in Data
    public void AddEventListener<T>(string eventName, EventCallback<T> listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallback<T>)this._events[eventName] + listener;
    }

    //To create a listener that doesnt need data, and can return a value back to wherever the function was Raised
    public void AddEventListener<T>(string eventName, EventCallBackWithReturn<T> listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallBackWithReturn<T>)this._events[eventName] + listener;
    }

    //To create a listener that does not take in Data
    public void AddEventListener(string eventName, EventCallback listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallback)this._events[eventName] + listener;
    }

    //To remove an event listener that takes in data
    public void RemoveEventListener<T>(string eventName, EventCallback<T> listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallback<T>)this._events[eventName] - listener;

        if (this._events[eventName] == null)
        {
            this.RemoveEvent(eventName);
        }
    }

    //To remove an event listener that does not take in data, and returns a value
    public void RemoveEventListener<T>(string eventName, EventCallBackWithReturn<T> listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallBackWithReturn<T>)this._events[eventName] - listener;

        if (this._events[eventName] == null)
        {
            this.RemoveEvent(eventName);
        }
    }

    //To remove an event listener that does not take in data
    public void RemoveEventListener(string eventName, EventCallback listener)
    {
        if (IsEventRegistered(eventName) == false)
        {
            this.RegisterEvent(eventName);
        }

        this._events[eventName] = (EventCallback)this._events[eventName] - listener;

        if (this._events[eventName] == null)
        {
            this.RemoveEvent(eventName);
        }
    }

    //To check if an event listener is already registered that takes in data
    public bool IsEventListenerRegistered<T>(string eventName, EventCallback<T> listener)
    {
        bool result = false;

        if (this.IsEventRegistered(eventName) == false)
        {
            return result;
        }

        Delegate[] list = this._events[eventName].GetInvocationList();
        foreach (Delegate d in list)
        {
            if ((EventCallback<T>)d == listener)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //To check if an event listener is already registered that does not take in data but does return a value
    public bool IsEventListenerRegistered<T>(string eventName, EventCallBackWithReturn<T> listener)
    {
        bool result = false;

        if (this.IsEventRegistered(eventName) == false)
        {
            return result;
        }

        Delegate[] list = this._events[eventName].GetInvocationList();
        foreach (Delegate d in list)
        {
            if ((EventCallBackWithReturn<T>)d == listener)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //To check if an event listener is already registered that does not take in data
    public bool IsEventListenerRegistered(string eventName, EventCallback listener)
    {
        bool result = false;

        if (this.IsEventRegistered(eventName) == false)
        {
            return result;
        }

        Delegate[] list = this._events[eventName].GetInvocationList();
        foreach (Delegate d in list)
        {
            if ((EventCallback)d == listener)
            {
                result = true;
                break;
            }
        }

        return result;
    }

    //To raise an event that takes in data
    public void Raise<T>(string eventName, T data)
    {
        if (this.IsEventRegistered(eventName) == false)
        {
            return;
        }

        (this._events[eventName] as EventCallback<T>)(data);
    }

    //To raise an event that does not take in data, but returns a value
    public T Raise<T>(string eventName)
    {
        if (this.IsEventRegistered(eventName) == false)
        {
            return default(T);
        }

        return (this._events[eventName] as EventCallBackWithReturn<T>)();
    }

    //To raise an event that does not take in data
    public void Raise(string eventName)
    {
        if (this.IsEventRegistered(eventName) == false)
        {
            return;
        }

        (this._events[eventName] as EventCallback)();
    }

}