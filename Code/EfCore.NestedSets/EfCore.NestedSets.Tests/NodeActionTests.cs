using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCore.NestedSets.Tests
{
    [TestClass]
    public class NodeActionTests
    {
        NestedSetManager<AppDbContext, Node, int, int?> _ns;
        private AppDbContext _db;
        public Node Animals { get; set; }
        public Node Humans { get; set; }
        public Node Males { get; set; }
        public Node Females { get; set; }
        public Node Cats { get; set; }
        public Node HouseCats { get; set; }
        public Node Tigers { get; set; }
        public Node Dogs { get; set; }
        public Node Kittens { get; set; }
        public Node Hairy { get; set; }
        public Node NonHairy { get; set; }
        public Node Pets { get; set; }
        public Node Josh { get; set; }
        private List<Node> _catsTree;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            DbSql.CreateDatabase();
            new AppDbContext().Database.Migrate();
        }

        [TestInitialize]
        public void SetUp()
        {
            Animals = null;
            Humans = null;
            Males = null;
            Females = null;
            Cats = null;
            HouseCats = null;
            Tigers = null;
            Dogs = null;
            Kittens = null;
            Hairy = null;
            NonHairy = null;
            Pets = null;
            Josh = null;
            // Clean up from the last test, but do this on set-up not
            // tear-down so it is possible to inspect the database with
            // the results of the last test
            DbSql.RunDbSql("DELETE FROM Nodes");
            _db = new AppDbContext();
            _ns = new NestedSetManager<AppDbContext, Node, int, int?>(_db, d => d.Nodes);
        }

        [TestCleanup]
        public void TearDown()
        {
            _db.Dispose();
        }

        private static Node NewNode(string name)
        {
            return new Node { Name = name };
        }

        [TestMethod]
        public void TestInsertRoot()
        {
            Animals = _ns.InsertRoot(NewNode("Animals"), NestedSetInsertMode.Right);
            AssertDb(Animals.RootId, new Node(Animals.Name, null, 0, 1, 2));
        }

        [TestMethod]
        public void TestInsertFirstLevelChild()
        {
            TestInsertRoot();
            Humans = _ns.InsertBelow(Animals.Id, NewNode("Humans"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 4),
                new Node(Humans.Name, Animals.Id, 1, 2, 3)
            );
        }

        [TestMethod]
        public void TestInsertSecondLevelChild()
        {
            TestInsertFirstLevelChild();
            Males = _ns.InsertBelow(Humans.Id, NewNode("Males"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 6),
                new Node(Humans.Name, Animals.Id, 1, 2, 5),
                new Node(Males.Name, Humans.Id, 2, 3, 4)
            );
        }

        [TestMethod]
        public void TestInsertSiblingToRight()
        {
            TestInsertSecondLevelChild();
            Females = _ns.InsertNextTo(Males.Id, NewNode("Females"), NestedSetInsertMode.Right);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 8),
                new Node(Humans.Name, Animals.Id, 1, 2, 7),
                new Node(Males.Name, Humans.Id, 2, 3, 4),
                new Node(Females.Name, Humans.Id, 2, 5, 6)
            );
        }

        [TestMethod]
        public void TestInsertSiblingToLeft()
        {
            TestInsertSiblingToRight();
            Cats = _ns.InsertNextTo(Humans.Id, NewNode("Cats"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 10),
                new Node(Cats.Name, Animals.Id, 1, 2, 3),
                new Node(Humans.Name, Animals.Id, 1, 4, 9),
                new Node(Males.Name, Humans.Id, 2, 5, 6),
                new Node(Females.Name, Humans.Id, 2, 7, 8)
            );
        }

        [TestMethod]
        public void TestInsertDeepChildToLeft()
        {
            TestInsertSiblingToLeft();
            HouseCats = _ns.InsertBelow(Cats.Id, NewNode("House cats"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 12),
                new Node(Cats.Name, Animals.Id, 1, 2, 5),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 4),
                new Node(Humans.Name, Animals.Id, 1, 6, 11),
                new Node(Males.Name, Humans.Id, 2, 7, 8),
                new Node(Females.Name, Humans.Id, 2, 9, 10)
            );
        }

        [TestMethod]
        public void TestInsertSiblingToRight2()
        {
            TestInsertDeepChildToLeft();
            Tigers = _ns.InsertNextTo(HouseCats.Id, NewNode("Tigers"), NestedSetInsertMode.Right);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 14),
                new Node(Cats.Name, Animals.Id, 1, 2, 7),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 4),
                new Node(Tigers.Name, Cats.Id, 2, 5, 6),
                new Node(Humans.Name, Animals.Id, 1, 8, 13),
                new Node(Males.Name, Humans.Id, 2, 9, 10),
                new Node(Females.Name, Humans.Id, 2, 11, 12)
            );
        }

        [TestMethod]
        public void TestInsertChildToRight()
        {
            TestInsertSiblingToRight2();
            Dogs = _ns.InsertBelow(Animals.Id, NewNode("Dogs"), NestedSetInsertMode.Right);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 16),
                new Node(Cats.Name, Animals.Id, 1, 2, 7),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 4),
                new Node(Tigers.Name, Cats.Id, 2, 5, 6),
                new Node(Humans.Name, Animals.Id, 1, 8, 13),
                new Node(Males.Name, Humans.Id, 2, 9, 10),
                new Node(Females.Name, Humans.Id, 2, 11, 12),
                new Node(Dogs.Name, Animals.Id, 1, 14, 15)
            );
        }

        [TestMethod]
        public void TestInsertChildToLeft()
        {
            TestInsertChildToRight();
            Kittens = _ns.InsertBelow(HouseCats.Id, NewNode("Kittens"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 18),
                new Node(Cats.Name, Animals.Id, 1, 2, 9),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 6),
                new Node(Kittens.Name, HouseCats.Id, 3, 4, 5),
                new Node(Tigers.Name, Cats.Id, 2, 7, 8),
                new Node(Humans.Name, Animals.Id, 1, 10, 15),
                new Node(Males.Name, Humans.Id, 2, 11, 12),
                new Node(Females.Name, Humans.Id, 2, 13, 14),
                new Node(Dogs.Name, Animals.Id, 1, 16, 17)
            );
        }

        [TestMethod]
        public void TestInsertChildToLeft2()
        {
            TestInsertChildToLeft();
            Hairy = _ns.InsertBelow(Males.Id, NewNode("Hairy"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 20),
                new Node(Cats.Name, Animals.Id, 1, 2, 9),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 6),
                new Node(Kittens.Name, HouseCats.Id, 3, 4, 5),
                new Node(Tigers.Name, Cats.Id, 2, 7, 8),
                new Node(Humans.Name, Animals.Id, 1, 10, 17),
                new Node(Males.Name, Humans.Id, 2, 11, 14),
                new Node(Hairy.Name, Males.Id, 3, 12, 13),
                new Node(Females.Name, Humans.Id, 2, 15, 16),
                new Node(Dogs.Name, Animals.Id, 1, 18, 19)
            );
        }

        [TestMethod]
        public void TestInsertChildToLeft3()
        {
            TestInsertChildToLeft2();
            NonHairy = _ns.InsertBelow(Males.Id, NewNode("Non-Hairy"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 22),
                new Node(Cats.Name, Animals.Id, 1, 2, 9),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 6),
                new Node(Kittens.Name, HouseCats.Id, 3, 4, 5),
                new Node(Tigers.Name, Cats.Id, 2, 7, 8),
                new Node(Humans.Name, Animals.Id, 1, 10, 19),
                new Node(Males.Name, Humans.Id, 2, 11, 16),
                new Node(NonHairy.Name, Males.Id, 3, 12, 13),
                new Node(Hairy.Name, Males.Id, 3, 14, 15),
                new Node(Females.Name, Humans.Id, 2, 17, 18),
                new Node(Dogs.Name, Animals.Id, 1, 20, 21)
            );
        }

        [TestMethod]
        public void TestInsertChildToLeft4()
        {
            TestInsertChildToLeft3();
            Josh = _ns.InsertBelow(Males.Id, NewNode("Josh"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 24),
                new Node(Cats.Name, Animals.Id, 1, 2, 9),
                new Node(HouseCats.Name, Cats.Id, 2, 3, 6),
                new Node(Kittens.Name, HouseCats.Id, 3, 4, 5),
                new Node(Tigers.Name, Cats.Id, 2, 7, 8),
                new Node(Humans.Name, Animals.Id, 1, 10, 21),
                new Node(Males.Name, Humans.Id, 2, 11, 18),
                new Node(Josh.Name, Males.Id, 3, 12, 13),
                new Node(NonHairy.Name, Males.Id, 3, 14, 15),
                new Node(Hairy.Name, Males.Id, 3, 16, 17),
                new Node(Females.Name, Humans.Id, 2, 19, 20),
                new Node(Dogs.Name, Animals.Id, 1, 22, 23)
            );
        }

        [TestMethod]
        public void TestDeleteSubTree()
        {
            TestInsertChildToLeft4();
            // Now let's delete Cats
            _catsTree = _ns.Delete(Cats.Id);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 16),
                new Node(Humans.Name, Animals.Id, 1, 2, 13),
                new Node(Males.Name, Humans.Id, 2, 3, 10),
                new Node(Josh.Name, Males.Id, 3, 4, 5),
                new Node(NonHairy.Name, Males.Id, 3, 6, 7),
                new Node(Hairy.Name, Males.Id, 3, 8, 9),
                new Node(Females.Name, Humans.Id, 2, 11, 12),
                new Node(Dogs.Name, Animals.Id, 1, 14, 15)
            );
        }

        [TestMethod]
        public void TestInsertChildToLeft5()
        {
            TestDeleteSubTree();
            // Create the pets node
            Pets = _ns.InsertBelow(Animals.Id, NewNode("Pets"), NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 18),
                new Node(Pets.Name, Animals.Id, 1, 2, 3),
                new Node(Humans.Name, Animals.Id, 1, 4, 15),
                new Node(Males.Name, Humans.Id, 2, 5, 12),
                new Node(Josh.Name, Males.Id, 3, 6, 7),
                new Node(NonHairy.Name, Males.Id, 3, 8, 9),
                new Node(Hairy.Name, Males.Id, 3, 10, 11),
                new Node(Females.Name, Humans.Id, 2, 13, 14),
                new Node(Dogs.Name, Animals.Id, 1, 16, 17)
            );
        }

        [TestMethod]
        public void TestReinsertDeletedTree()
        {
            TestInsertChildToLeft5();
            // And insert the removed Cats node under Pets
            _catsTree = _ns.InsertBelow(Pets.Id, _catsTree, NestedSetInsertMode.Right);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 26),
                new Node(Pets.Name, Animals.Id, 1, 2, 11),
                new Node(Cats.Name, Pets.Id, 2, 3, 10),
                new Node(HouseCats.Name, Cats.Id, 3, 4, 7),
                new Node(Kittens.Name, HouseCats.Id, 4, 5, 6),
                new Node(Tigers.Name, Cats.Id, 3, 8, 9),
                new Node(Humans.Name, Animals.Id, 1, 12, 23),
                new Node(Males.Name, Humans.Id, 2, 13, 20),
                new Node(Josh.Name, Males.Id, 3, 14, 15),
                new Node(NonHairy.Name, Males.Id, 3, 16, 17),
                new Node(Hairy.Name, Males.Id, 3, 18, 19),
                new Node(Females.Name, Humans.Id, 2, 21, 22),
                new Node(Dogs.Name, Animals.Id, 1, 24, 25)
            );
        }

        [TestMethod]
        public void TestMoveNodeToParentLeft1()
        {
            TestReinsertDeletedTree();
            _ns.MoveToParent(Humans.Id, Pets.Id, NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 26),
                new Node(Pets.Name, Animals.Id, 1, 2, 23),
                new Node(Humans.Name, Pets.Id, 2, 3, 14),
                new Node(Males.Name, Humans.Id, 3, 4, 11),
                new Node(Josh.Name, Males.Id, 4, 5, 6),
                new Node(NonHairy.Name, Males.Id, 4, 7, 8),
                new Node(Hairy.Name, Males.Id, 4, 9, 10),
                new Node(Females.Name, Humans.Id, 3, 12, 13),
                new Node(Cats.Name, Pets.Id, 2, 15, 22),
                new Node(HouseCats.Name, Cats.Id, 3, 16, 19),
                new Node(Kittens.Name, HouseCats.Id, 4, 17, 18),
                new Node(Tigers.Name, Cats.Id, 3, 20, 21),
                new Node(Dogs.Name, Animals.Id, 1, 24, 25)
            );
        }

        [TestMethod]
        public void TestMoveToSiblingRight()
        {
            TestMoveNodeToParentLeft1();
            _ns.MoveToSibling(Pets.Id, Dogs.Id, NestedSetInsertMode.Right);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 26),
                new Node(Dogs.Name, Animals.Id, 1, 2, 3),
                new Node(Pets.Name, Animals.Id, 1, 4, 25),
                new Node(Humans.Name, Pets.Id, 2, 5, 16),
                new Node(Males.Name, Humans.Id, 3, 6, 13),
                new Node(Josh.Name, Males.Id, 4, 7, 8),
                new Node(NonHairy.Name, Males.Id, 4, 9, 10),
                new Node(Hairy.Name, Males.Id, 4, 11, 12),
                new Node(Females.Name, Humans.Id, 3, 14, 15),
                new Node(Cats.Name, Pets.Id, 2, 17, 24),
                new Node(HouseCats.Name, Cats.Id, 3, 18, 21),
                new Node(Kittens.Name, HouseCats.Id, 4, 19, 20),
                new Node(Tigers.Name, Cats.Id, 3, 22, 23)
            );
        }

        [TestMethod]
        public void TestMoveNodeToParentLeft2()
        {
            TestMoveToSiblingRight();
            _ns.MoveToParent(Humans.Id, Animals.Id, NestedSetInsertMode.Left);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 26),
                new Node(Humans.Name, Animals.Id, 1, 2, 13),
                new Node(Males.Name, Humans.Id, 2, 3, 10),
                new Node(Josh.Name, Males.Id, 3, 4, 5),
                new Node(NonHairy.Name, Males.Id, 3, 6, 7),
                new Node(Hairy.Name, Males.Id, 3, 8, 9),
                new Node(Females.Name, Humans.Id, 2, 11, 12),
                new Node(Dogs.Name, Animals.Id, 1, 14, 15),
                new Node(Pets.Name, Animals.Id, 1, 16, 25),
                new Node(Cats.Name, Pets.Id, 2, 17, 24),
                new Node(HouseCats.Name, Cats.Id, 3, 18, 21),
                new Node(Kittens.Name, HouseCats.Id, 4, 19, 20),
                new Node(Tigers.Name, Cats.Id, 3, 22, 23)
            );
        }

        [TestMethod]
        public void TestDeleteSubTree2()
        {
            TestMoveNodeToParentLeft2();
            _ns.Delete(Pets.Id);
            AssertDb(
                Animals.RootId,
                new Node(Animals.Name, null, 0, 1, 16),
                new Node(Humans.Name, Animals.Id, 1, 2, 13),
                new Node(Males.Name, Humans.Id, 2, 3, 10),
                new Node(Josh.Name, Males.Id, 3, 4, 5),
                new Node(NonHairy.Name, Males.Id, 3, 6, 7),
                new Node(Hairy.Name, Males.Id, 3, 8, 9),
                new Node(Females.Name, Humans.Id, 2, 11, 12),
                new Node(Dogs.Name, Animals.Id, 1, 14, 15)
            );
        }

        [TestMethod]
        public void TestMultipleHierarchies()
        {
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
            TestAnimalsHierarchy();
        }

        private void TestAnimalsHierarchy()
        {
            TestDeleteSubTree2();
        }

        private static void AssertDb(int? rootId, params Node[] expectedNodes)
        {
            using (var db = new AppDbContext())
            {
                var nodes = db.Nodes.Where(n => n.RootId == rootId);
                Assert.AreEqual(expectedNodes.Length, nodes.Count());
                for (var i = 0; i < expectedNodes.Length; i++)
                {
                    var node = nodes.SingleOrDefault(n => n.Name == expectedNodes[i].Name);
                    Assert.AreEqual(rootId, node.RootId);
                    Assert.AreEqual(expectedNodes[i].Left, node.Left);
                    Assert.AreEqual(expectedNodes[i].Right, node.Right);
                    Assert.AreEqual(expectedNodes[i].ParentId, node.ParentId);
                    Assert.AreEqual(expectedNodes[i].Level, node.Level);
                }
            }
        }
    }
}