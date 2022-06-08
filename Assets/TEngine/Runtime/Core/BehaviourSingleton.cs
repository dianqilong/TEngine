﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEngine
{
    public class BehaviourSingleton<T> : BaseBehaviourSingleton where T : BaseBehaviourSingleton, new()
    {
        private static T sInstance;
        public static T Instance
        {
            get
            {
                if (null == sInstance)
                {
                    sInstance = new T();
                    TLogger.LogAssert(sInstance != null);
                    sInstance.Awake();
                    RegSingleton(sInstance);
                }

                return sInstance;
            }
        }

        private static void RegSingleton(BaseBehaviourSingleton inst)
        {
            BehaviourSingleSystem.Instance.RegSingleton(inst);
        }
    }

    public class BaseBehaviourSingleton
    {
        public bool IsStart = false;

        public virtual void Awake()
        {
        }

        public virtual bool IsHaveLateUpdate()
        {
            return false;
        }

        public virtual void Start()
        {
        }

        public virtual void Update()
        {
        }

        public virtual void LateUpdate()
        {
        }

        public virtual void Destroy()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }
    }

    public class BehaviourSingleSystem : BaseLogicSys<BehaviourSingleSystem>
    {
        List<BaseBehaviourSingleton> m_listInst = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listStart = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listUpdate = new List<BaseBehaviourSingleton>();
        List<BaseBehaviourSingleton> m_listLateUpdate = new List<BaseBehaviourSingleton>();

        public void RegSingleton(BaseBehaviourSingleton inst)
        {
            TLogger.LogAssert(!m_listInst.Contains(inst));
            m_listInst.Add(inst);
            m_listStart.Add(inst);
        }

        public override void OnUpdate()
        {
            var listStart = m_listStart;

            var listToUpdate = m_listUpdate;
            var listToLateUpdate = m_listLateUpdate;

            if (listStart.Count > 0)
            {
                for (int i = 0; i < listStart.Count; i++)
                {
                    var inst = listStart[i];
                    TLogger.LogAssert(!inst.IsStart);

                    inst.IsStart = true;
                    inst.Start();
                    listToUpdate.Add(inst);

                    if (inst.IsHaveLateUpdate())
                    {
                        listToLateUpdate.Add(inst);
                    }
                }

                listStart.Clear();
            }

            var listUpdateCnt = listToUpdate.Count;
            for (int i = 0; i < listUpdateCnt; i++)
            {
                var inst = listToUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.Update();
                TProfiler.EndFirstSample();
            }
        }

        public override void OnLateUpdate()
        {
            var listLateUpdate = m_listLateUpdate;
            var listLateUpdateCnt = listLateUpdate.Count;
            for (int i = 0; i < listLateUpdateCnt; i++)
            {
                var inst = listLateUpdate[i];

                TProfiler.BeginFirstSample(inst.GetType().FullName);
                inst.LateUpdate();
                TProfiler.EndFirstSample();
            }
        }

        public override void OnDestroy()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.Destroy();
            }
        }

        public override void OnPause()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.OnPause();
            }
        }

        public override void OnResume()
        {
            for (int i = 0; i < m_listInst.Count; i++)
            {
                var inst = m_listInst[i];
                inst.OnResume();
            }
        }
    }
}
