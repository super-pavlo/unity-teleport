﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeBox.Teleport.Unity;

namespace DeBox.Teleport.Tests
{
    

    public class TestBoids : MonoBehaviour
    {
        [SerializeField]
        private TeleportManager _manager = null;

        [SerializeField]
        private GameObject _serverPrefab = null;

        [SerializeField]
        private int _spawnedObjectCount = 10;

        [SerializeField]
        private float _spawnDuration = 60;

        [SerializeField]
        private float _despawnAfter = 120;

        private List<GameObject> _spawnedServerInstances = new List<GameObject>();

        private float RandomizeFloat(float magnitude)
        {
            return Random.Range(-magnitude, magnitude);
        }

        private Vector3 RandomizeVector(float magnitude)
        {
            return new Vector3(RandomizeFloat(magnitude), RandomizeFloat(magnitude), RandomizeFloat(magnitude));
        }

        private IEnumerator Start()
        {
            Debug.Log("Test will start in 3 seconds...");
            yield return new WaitForSeconds(3);
            int spawnCount = _spawnedObjectCount;
            float duration = _spawnDuration;
            _manager.StartServer();

            _manager.RegisterServerMessage<TestMessage>();
            Debug.Log("Waiting for server to start...");
            while (!_manager.IsServerListening)
            {
                yield return null;
            }
            Debug.Log("Server started, spawning objects...");
            while (spawnCount-- > 0)
            {
                _spawnedServerInstances.Add(_manager.ServerSideSpawn(_serverPrefab, RandomizeVector(60)));
            }
            yield return new WaitForSeconds(2);
            Debug.Log("Waiting for client to connect..");
            _manager.ConnectClient();
            _manager.RegisterClientMessage<TestMessage>();
            while (!_manager.IsServerListening || _manager.ClientState != TeleportClientProcessor.StateType.Connected)
            {
                yield return null;
            }
            Debug.Log("Client connected!");
            bool didDispawn = false;

            while (duration > 0)
            {
                yield return new WaitForSeconds(0.01f);

                if (_spawnDuration - duration >= _despawnAfter && !didDispawn)
                {
                    foreach (var instance in _spawnedServerInstances)
                    {
                        _manager.ServerSideDespawn(instance);
                    }
                    didDispawn = true;
                    _spawnedServerInstances.Clear();
                }
                duration -= Time.deltaTime;
            }
            foreach (var instance in _spawnedServerInstances)
            {
                _manager.ServerSideDespawn(instance);
            }
            yield return new WaitForSeconds(5);
            _manager.DisconnectClient();
            _manager.StopServer();
        }

    }



}
