 using System.Collections;
using System.Collections.Generic;
using QFramework;
using UnityEngine;

namespace MyGameNamespace
{
    

public class RegisterManager : Architecture<RegisterManager>
{
    protected override void Init()
    {
         //RegisterModel<IGameModel>(new GameModel());
        
        
         //RegisterSystem<IDmManagerSystem>(new DmManagerSystem());
         //RegisterUtility<IBallWarTools>(new BallWarTools());
        // RegisterSystem<IUpDataSystem>(new UpDataSystem());
         RegisterSystem<IObjectPoolSystem>(new ObjectPoolSystem());
        // RegisterSystem<IEquipmentManagerSystem>(new EquipmentManagerSystem());
        // RegisterSystem<ICreatePlaySystem>(new CreatePlaySystem());
          RegisterSystem<ITimeSystem>(new TimeSystem());
        // RegisterSystem<IRankManagerSystem>(new RankManagerSystem());
        // RegisterSystem<ISkillManagerSystem>(new SkillManagerSystem());
        //
          //RegisterSystem<IBulletFactorySystem>(new BulletFactorySystem()); 
         // RegisterSystem<IQTreeCollisionSystem>(new QTreeCollisionSystem());
          //RegisterSystem<ITipsManagerSystem>(new TipsManagerSystem());
          
          //RegisterSystem<IResearchSystem>(new ResearchSystem());
            //RegisterSystem<ICardGachaSystem>(new CardGachaSystem());
          
        //  RegisterSystem<IDelayTimeSystem>(new DelayTimeSystem());
        
    }
}
}