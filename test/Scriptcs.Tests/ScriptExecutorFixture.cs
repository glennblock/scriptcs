namespace Scriptcs.Tests
{
    using System.ComponentModel.Composition;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using Moq;
    using System;
    using System.Collections.Generic;
    using Scriptcs.Contracts;

    [TestClass]
    public class ScriptExecutorFixture
    {
        private MockRepository mockRepository;
        private Mock<IFileSystem> fileSystem;

        [TestInitialize]
        public void InitializeMocks() 
        {
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.fileSystem = this.mockRepository.Create<IFileSystem>();
        }

        [TestMethod]
        public void ShouldAddSystemAndSystemCoreReferencesToEngine()
        {
            // arrange
            var scriptEngine = this.mockRepository.Create<IScriptEngine>();
            var session = this.mockRepository.Create<ISession>();

            scriptEngine.Setup(e => e.AddReference("System")).Verifiable();
            scriptEngine.Setup(e => e.AddReference("System.Core")).Verifiable();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            this.fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            scriptEngine.Verify(e => e.AddReference("System"), Times.Once());
            scriptEngine.Verify(e => e.AddReference("System.Core"), Times.Once());
        }

        [TestMethod]
        public void ShouldSetEngineBaseDirectoryBasedOnCurrentDirectoryAndBinFolder()
        {
            // arrange
            var scriptEngine = this.mockRepository.Create<IScriptEngine>();
            
            var session = this.mockRepository.Create<ISession>();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            this.fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            scriptEngine.SetupProperty(e => e.BaseDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            Assert.AreEqual(currentDirectory + @"\bin", scriptEngine.Object.BaseDirectory);
        }

        [TestMethod]
        public void ShouldCreateCurrentDirectoryIfItDoesNotExist()
        {
            // arrange
            var scriptEngine = this.mockRepository.Create<IScriptEngine>();

            var session = this.mockRepository.Create<ISession>();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            var currentDirectory = @"C:\";
            this.fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var binDirectory = currentDirectory + @"\bin";

            this.fileSystem.Setup(fs => fs.DirectoryExists(binDirectory)).Returns(false).Verifiable();
            this.fileSystem.Setup(fs => fs.CreateDirectory(binDirectory)).Verifiable();

            var scriptExecutor = this.CreateScriptExecutor(
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            this.fileSystem.Verify(fs => fs.DirectoryExists(binDirectory), Times.Once());
            this.fileSystem.Verify(fs => fs.CreateDirectory(binDirectory), Times.Once());
        }

        [TestMethod]
        public void ShouldExecuteScriptReadFromFileInSession()
        {
            // arrange
            string code = Guid.NewGuid().ToString();
            
            var scriptEngine = this.mockRepository.Create<IScriptEngine>();

            var session = this.mockRepository.Create<ISession>();
            scriptEngine.Setup(e => e.CreateSession()).Returns(session.Object);

            session.Setup(s => s.Execute(code)).Returns(null).Verifiable();

            var currentDirectory = @"C:\";
            this.fileSystem.Setup(fs => fs.CurrentDirectory).Returns(currentDirectory);

            var scriptExecutor = this.CreateScriptExecutor(
                new ExportFactory<IScriptEngine>(
                    () => new Tuple<IScriptEngine, Action>(scriptEngine.Object, null)));

            var scriptName = "script.csx";
            var paths = new string[0];
            IEnumerable<IScriptcsRecipe> recipes = null;

            this.fileSystem.Setup(fs => fs.ReadFile(currentDirectory + @"\" + scriptName)).Returns(code).Verifiable();

            // act
            scriptExecutor.Execute(scriptName, paths, recipes);

            // assert
            this.fileSystem.Verify(fs => fs.ReadFile(currentDirectory + @"\" + scriptName), Times.Once());
            session.Verify(s => s.Execute(code), Times.Once());
        }

        private ScriptExecutor CreateScriptExecutor(ExportFactory<IScriptEngine> scriptEngineFactory) 
        {
            return new ScriptExecutor(this.fileSystem.Object, scriptEngineFactory);
        }
    }
}