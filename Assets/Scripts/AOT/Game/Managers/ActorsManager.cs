using System.Collections.Generic;
using UnityEngine;

namespace FPS.Game.Managers
{
    public class ActorsManager : MonoBehaviour
    {
        public List<Actor> actors { get; private set; }
        public GameObject player { get; private set; }

        public void SetPlayer(GameObject player) => this.player = player;

        void Awake()
        {
            actors = new List<Actor>();
        }
    }
}