using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Text.Unicode;
using System.Xml.Serialization;

namespace YupGames_ECS
{
    public class Scene
    {
        public string name;
        public Dictionary<int,Entity> sceneEntities;

        public Scene(string name)
        {
            this.name = name;
            sceneEntities = new Dictionary<int,Entity>();
        }
        ~Scene()
        {
            GC.Collect();
        }

        public string Serialize()
        {
            JsonSerializerSettings ss = new JsonSerializerSettings();
            ss.Formatting = Formatting.Indented;
            ss.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.SerializeObject(this,ss);
        }
        public static Scene Deserialize(string serialized)
        {
            JsonSerializerSettings ss = new JsonSerializerSettings();
            ss.Formatting = Formatting.Indented;
            ss.TypeNameHandling = TypeNameHandling.All;
            return JsonConvert.DeserializeObject<Scene>(serialized,ss);
        }
        public Entity? FindByUID(int uid)
        {
            try
            {
                return sceneEntities[uid];
            }
            catch
            {
                return null;
            }
        }
        public Entity? FindByName(string name)
        {
            foreach (Entity entity in sceneEntities.Values)
            {
                if (entity.name == name)
                    return entity;
            }
            return null;
        }

        public void DestroyAll()
        {
            foreach(var ent in sceneEntities)
            {
                DestroyEntity(ent.Value);
            }
            GC.Collect();
        }

        public void DestroyEntity(Entity ent)
        {
            if(ent.parentUID != -1)
            {
                var parentEntity = FindByUID((int)ent.parentUID);
                parentEntity?.children.Remove(ent.UID);
            }
            sceneEntities.Remove(ent.UID);
            foreach (var ch in ent.children)
            {
                Entity? ent2 = FindByUID(ch);   
                if(ent2 != null)
                {
                    foreach (var comp in ent2.components)
                    {
                        comp.DestroyThis();
                    }
                    DestroyEntity(ent2);
                }
            }
            ent.children.Clear();
            foreach (var comp in ent.components)
            {
                comp.DestroyThis();
            }
            ent.components.Clear();
        }

        public Entity SpawnEntity(string name, Entity parent)
        {
            int freeUID = new Random().Next(0, int.MaxValue);
            while(sceneEntities.ContainsKey(freeUID))
            {
                freeUID = new Random().Next(0, int.MaxValue);
            }
            Entity ent = new Entity(name, parent,freeUID);
            parent.children.Add(ent.UID);
            sceneEntities.Add(ent.UID,ent);
            return ent;
        }
    }

    public class Entity
    {
        public int UID;
        public string name;
        public int parentUID;
        public List<int> children;
        public List<Component> components;

        public Entity(string name, Entity parent, int UID)
        {
            this.name = name;
            this.parentUID = parent.UID;
            this.UID = UID;
            this.children = new List<int>();
            this.components = new List<Component>();
        }

        public void AddComponent(Component comp)
        {
            if(!components.Contains(comp))
            {
                components.Add(comp);
                comp.attchedToUID = this.UID;
            }
        }
        public Component? GetComponent<T>()
        {
            foreach(Component comp in components)
            {
                if(comp.GetType() == typeof(T))
                {
                    return comp;
                }
            }
            return null;
        }
        public void DestroyComponent(Component comp)
        {
            components.Remove(comp);
            comp.DestroyThis();
        }
        public void AddChildren(Entity obj)
        {
            children.Add(obj.UID);
            obj.parentUID = this.UID;
        }
        public bool RemoveChildren(Entity obj)
        {
            if(children.Contains(obj.UID))
            {
                children.Remove(obj.UID);
                obj.parentUID = -1;
                return true;
            }
            return false;
        }
    }

    public class Component
    {
        public int attchedToUID;

        public virtual void DestroyThis()
        {

        }
    }
}