YupGames Entity Component system with scene and serialization support. Written in c# but have plans to rewrite in c++.
Usage:

```cs
using System.IO.Compression;
using System.Numerics;
using System.Text;
using YupGames_ECS;

class Transform : Component
{
    public Vector3 position = new Vector3();
    public Vector3 rotation = new Vector3();
    public Vector3 scale = new Vector3();
    public override void DestroyThis()
    {
        base.DestroyThis();
        position = Vector3.Zero;
        rotation = Vector3.Zero;
        scale = Vector3.Zero;
    }
    public Transform(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        this.position = pos;
        this.rotation = rot;
        this.scale = scale;
    }
}


class MainClass
{
    public static float Random(int min,int max)
    {
        var random = new Random();
        return random.Next(min,max) - random.NextSingle();
    }

    public static void Main()
    {
        Scene scen = new Scene("MAIN");

        for (int i = 0; i < 50000; i++)
        {
            var ent = scen.SpawnEntity("gameobject " + i, null);
            Transform ts = new Transform(Vector3.One * Random(-200000, 200000), Vector3.Zero, Vector3.One);
            ent.AddComponent(ts);

            var ent2 = scen.SpawnEntity("gameobject child " + i, ent);
            Transform ts1 = new Transform(Vector3.One * Random(-200000, 200000), Vector3.Zero, Vector3.One);
            ent2.AddComponent(ts1);
        }

        foreach (var ent in scen.sceneEntities)
        {
            Console.WriteLine("Entity name: " + ent.Value.name);
            Console.WriteLine("Entity pos: " + ((Transform)ent.Value.GetComponent<Transform>()).position.ToString());
            var entChildCount = ent.Value.children.Count;
            if (entChildCount > 0)
            {
                Console.WriteLine("Entity child name: " + scen.FindByUID(ent.Value.children[0]).name);
                Transform tr = (Transform)scen.FindByUID(ent.Value.children[0]).GetComponent<Transform>();
                Console.WriteLine("Entity child pos: " + tr.position.ToString());
            }
        }

        File.WriteAllText("./main.jsonc", scen.Serialize());
        scen.DestroyAll();
        GC.Collect();



        var scene = Scene.Deserialize(File.ReadAllText("./main.yscene"));

        foreach (var ent in scene.sceneEntities)
        {
            Console.WriteLine("Entity name: \"" + ent.Value.name + "\"");
            Console.WriteLine("Entity pos: " + ((Transform)ent.Value.GetComponent<Transform>()).position.ToString());
        }

        Console.ReadKey();
    }
}
```
