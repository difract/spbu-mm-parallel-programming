using Microsoft.VisualStudio.TestTools.UnitTesting;
using task4.Models;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace UnitTestExamSystem
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRunner()
        {
            SingleDataTest(SetType.LazySetSystem);
            SingleDataTest(SetType.CoarseSetSystem);
            MultipleDataTest(SetType.LazySetSystem);
            MultipleDataTest(SetType.CoarseSetSystem);
            ParallelMultipleDataTest(SetType.LazySetSystem);
            ParallelMultipleDataTest(SetType.CoarseSetSystem);
        }
        public void SingleDataTest(SetType type)
        {
            ExamSystem<StudentData> examSystem = new ExamSystem<StudentData>(type);
            var element = new StudentData(1, 1);
            Assert.IsTrue(examSystem.Count() == 0);
            examSystem.Add(element);
            Assert.IsTrue(examSystem.Contains(element));
            Assert.IsTrue(examSystem.Count() == 1);
            examSystem.Remove(element);
            Assert.IsFalse(examSystem.Contains(element));
            Assert.IsTrue(examSystem.Count() == 0);
        }

        public void MultipleDataTest(SetType type)
        {
            int count = 200;
            List<StudentData> data = new List<StudentData>();
            ExamSystem<StudentData> examSystem = new ExamSystem<StudentData>(type);
            Random generator = new Random();
            for (int i = 0; i < count; i++)
            {
                StudentData element = new StudentData(generator.Next(0, count), generator.Next(0, count));
                data.Add(element);
                examSystem.Add(element);
            }
            Assert.IsTrue(examSystem.Count() == count);
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(examSystem.Contains(data[i]));
            }
            int num = generator.Next(0, count);
            examSystem.Remove(data[num]);
            Assert.IsFalse(examSystem.Contains(data[num]));
            Assert.IsTrue(examSystem.Count() == count - 1);
        }

        public void ParallelMultipleDataTest(SetType type)
        {
            int count = 200;
            List<StudentData> data = new List<StudentData>();
            ExamSystem<StudentData> examSystem = new ExamSystem<StudentData>(type);
            Random generator = new Random();
            for (int i = 0; i < count; i++)
            {
                data.Add(new StudentData(generator.Next(0, count), generator.Next(0, count)));
            }
            Parallel.For(0, count, i => examSystem.Add(data[i]));
            Assert.IsTrue(examSystem.Count() == count);
            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(examSystem.Contains(data[i]));
            }
            int num = generator.Next(0, count);
            Parallel.For(0, count/2, i => examSystem.Remove(data[i]));
            for (int i = 0; i < count; i++)
            {
                if(i < count/2)
                    Assert.IsFalse(examSystem.Contains(data[i]));
                else
                    Assert.IsTrue(examSystem.Contains(data[i]));
            }
            Assert.IsTrue(examSystem.Count() == count / 2);
        }
    }
}
