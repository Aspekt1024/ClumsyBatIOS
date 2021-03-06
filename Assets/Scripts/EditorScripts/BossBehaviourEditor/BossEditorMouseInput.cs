﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BossEditorMouseInput {

    private enum MouseButtons
    {
        LeftClick = 0, RightClick = 1, MiddleClick = 2
    }

    // TODO auto search nodetypes based on inheritance tree?
    public enum NodeTypes
    {
        Start,
        End,
        SaySomething,
        Jump,
        Die
    }
    
    public enum NodeMenuSelections
    {
        DoNothing,
        DeleteNode
    }

    private BossEditor _editor;

    private BaseNode _mouseDownNode;
    private int _outputIndex;
    private BaseNode _mouseUpNode;

    private Vector2 _mousePos;
    
    public BossEditorMouseInput(BossEditor editor)
    {
        _editor = editor;
    }

	public void ProcessMouseEvents(Event e)
    {
        _mousePos = e.mousePosition;

        // TODO move this to separate class?
        if (e.type == EventType.MouseDown)
        {
            if (ActionMouseDown(e.button))
            {
                e.Use();
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            ActionMouseUp(e.button);
        }

    }
    
    private bool ActionMouseDown(int button)
    {
        _mouseDownNode = _editor.GetSelectedNode();
        bool clickActioned = false;
        switch (button)
        {
            case (int)MouseButtons.LeftClick:
                clickActioned = ActionLeftMouseDown();
                break;
            case (int)MouseButtons.RightClick:
                ActionRightMouseDown();
                clickActioned = true;
                break;
            case (int)MouseButtons.MiddleClick:
                // nothing?
                break;
        }
        return clickActioned;
    }

    private void ActionMouseUp(int button)
    {
        _mouseUpNode = _editor.GetSelectedNode();
        switch (button)
        {
            case (int)MouseButtons.LeftClick:
                ActionLeftMouseUp();
                break;
            case (int)MouseButtons.RightClick:
                // no action on mouseup rightclick
                break;
            case (int)MouseButtons.MiddleClick:
                // nothing?
                break;
        }
    }

    private bool ActionLeftMouseDown()
    {
        if (_mouseDownNode == null) return false;
        
        _outputIndex = _mouseDownNode.OutputClicked(_mousePos);
        int inputIndex = _mouseDownNode.InputClicked(_mousePos);
        if (_outputIndex >= 0)
        {
            _editor.ConnectionMode = true;
        }
        else if (inputIndex >= 0)
        {
            if (_mouseDownNode.InputIsConnected(inputIndex))
            {
                var output = _mouseDownNode.GetConnectedInterfaceFromInput(inputIndex);
                _mouseDownNode.DisconnectInput(inputIndex);

                _mouseDownNode = output.connectedNode;
                _outputIndex = output.connectedIndex;
                _mouseDownNode.DisconnectOutput(_outputIndex);

                _editor.SetSelectedNode(output.connectedNode);
                _editor.ConnectionMode = true;
            }
        }

        return false; // TODO remove this? tells the controller whether to Use() this event.
    }

    private void ActionRightMouseDown()
    {
        if (_mouseDownNode == null)
        {
            BossEditorContextMenus.ShowMenu(ContextCallback);
        }
        else
        {
            BossEditorContextMenus.ShowNodeMenu(ContextCallback);
        }
    }

    private void ActionLeftMouseUp()
    {
        if (_mouseUpNode != null)
        {
            int inputIndex = _mouseUpNode.GetReleasedNode(_mousePos);
            if (_editor.ConnectionMode && inputIndex >= 0)
            {
                BaseNode.InterfaceType output = new BaseNode.InterfaceType();
                output.connectedNode = _mouseDownNode;
                output.connectedIndex = _outputIndex;
                _mouseUpNode.SetInput(inputIndex, output);
            }
        }
        _editor.ConnectionMode = false;
    }
    
    public void ContextCallback(object obj)
    {
        if (obj.GetType().Equals(typeof(NodeTypes)))
        {
            BossEditorEvents.CreateNode((NodeTypes)obj);
        }
        else if (obj.GetType().Equals(typeof(NodeMenuSelections)))
        {
            ActionNodeMenuSelection((NodeMenuSelections)obj);
        }
    }
    
    private void ActionNodeMenuSelection(NodeMenuSelections selection)
    {
        switch (selection)
        {
            case NodeMenuSelections.DoNothing:
                // as it says
                break;
            case NodeMenuSelections.DeleteNode:
                _editor.RemoveNode(_mouseDownNode);
                break;
        }
    }
}
