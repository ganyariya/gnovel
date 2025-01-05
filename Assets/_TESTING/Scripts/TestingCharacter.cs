using System.Collections;
using System.Collections.Generic;
using Core.Characters;
using UnityEngine;

namespace Testing
{
    public class TestingCharacter : MonoBehaviour
    {
        void Start()
        {
            Character ganyariya = CharacterManager.instance.CreateCharacter("ganyariya");
            Character notFound = CharacterManager.instance.CreateCharacter("notFound");
        }
    }
}