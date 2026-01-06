import dataclasses
import json
import os
import re
from abc import ABC, abstractmethod
from pathlib import Path
from typing import Dict, List, Any, Optional
from functools import wraps


# ==================== 验证规则 ====================
class ValidationRules:
    """验证规则集中管理"""

    VERSION_PATTERN = r"^\d+\.\d+\.\d+$"
    MAX_VERSION = "1.0.0"

    @staticmethod
    def validate_version(version: str) -> None:
        """验证版本号格式和范围"""
        if not re.match(ValidationRules.VERSION_PATTERN, version):
            raise ValueError(f"Invalid version format: {version}. Expected format: X.Y.Z")

        # 比较版本号
        def parse_version(v: str) -> tuple:
            return tuple(map(int, v.split(".")))

        if parse_version(version) > parse_version(ValidationRules.MAX_VERSION):
            raise ValueError(f"Version {version} exceeds maximum allowed version {ValidationRules.MAX_VERSION}")


# ==================== 文件管理器:统一文件操作 ====================
class FileManager:
    """统一处理所有文件操作和异常"""

    @staticmethod
    def load_json(file_path: Path) -> Dict:
        """加载JSON文件"""
        if not file_path.exists():
            raise FileNotFoundError(f"File not found: {file_path}")

        with open(file_path, "r", encoding="utf-8") as f:
            return json.load(f)

    @staticmethod
    def save_json(file_path: Path, data: Dict) -> None:
        """保存JSON文件"""
        file_path.parent.mkdir(parents=True, exist_ok=True)
        with open(file_path, "w", encoding="utf-8") as f:
            json.dump(data, f, indent=4, ensure_ascii=False)

    @staticmethod
    def ensure_dir(dir_path: Path) -> None:
        """确保目录存在"""
        dir_path.mkdir(parents=True, exist_ok=True)


# ==================== 数据类:配置 ====================
@dataclasses.dataclass
class ModelConfig:
    """模型配置数据类"""

    model_name: str
    model_type: str
    description: str
    parameters: Dict[str, Any]
    project_path: Path

    @property
    def model_path(self) -> Path:
        """模型目录路径"""
        return self.project_path / self.model_name

    @property
    def config_path(self) -> Path:
        """模型配置文件路径"""
        return self.model_path / f"{self.model_name}.json"

    def to_dict(self) -> Dict:
        """转换为字典"""
        return {"model_name": self.model_name, "model_type": self.model_type, "description": self.description, "parameters": self.parameters}

    @staticmethod
    def from_dict(data: Dict, project_path: Path) -> "ModelConfig":
        """从字典创建"""
        return ModelConfig(model_name=data.get("model_name", ""), model_type=data.get("model_type", ""), description=data.get("description", ""), parameters=data.get("parameters", {}), project_path=project_path)


@dataclasses.dataclass
class ProjectConfig:
    """项目配置数据类"""

    project_name: str
    version: str
    project_path: Path
    config_path: Path

    def __post_init__(self):
        """初始化后验证"""
        ValidationRules.validate_version(self.version)

    def to_dict(self) -> Dict:
        """转换为字典"""
        return {"name": self.project_name, "version": self.version}

    @staticmethod
    def from_dict(data: Dict, project_path: Path, config_path: Path) -> "ProjectConfig":
        """从字典创建"""
        version = data.get("version", "1.0.0")
        ValidationRules.validate_version(version)
        return ProjectConfig(project_name=data.get("name", ""), version=version, project_path=project_path, config_path=config_path)


# ==================== 模型基类 ====================
class BaseModel(ABC):
    """模型基类"""

    def __init__(self, config: ModelConfig):
        self.config = config

    @abstractmethod
    def execute(self, *args, **kwargs):
        """执行模型特定操作"""
        pass

    def load_config(self) -> None:
        """加载模型配置"""
        data = FileManager.load_json(self.config.config_path)
        self.config.model_name = data.get("model_name", "")
        self.config.model_type = data.get("model_type", "")
        self.config.description = data.get("description", "")
        self.config.parameters = data.get("parameters", {})

    def save_config(self) -> None:
        """保存模型配置"""
        FileManager.save_json(self.config.config_path, self.config.to_dict())


# ==================== 具体模型实现 ====================
class LoggerModel(BaseModel):
    """日志记录器模型"""

    def __init__(self, config: ModelConfig):
        super().__init__(config)
        # 默认参数
        if "log_level" not in self.config.parameters:
            self.config.parameters["log_level"] = "INFO"
        if "max_entries" not in self.config.parameters:
            self.config.parameters["max_entries"] = 1000
        if "logs" not in self.config.parameters:
            self.config.parameters["logs"] = []

    def execute(self, message: str, level: str = "INFO") -> None:
        """记录日志"""
        log_entry = f"[{level}] {message}"
        print(f"[LoggerModel] {log_entry}")

        # 保存到参数中
        self.config.parameters["logs"].append(log_entry)

        # 限制日志数量
        max_entries = self.config.parameters.get("max_entries", 1000)
        if len(self.config.parameters["logs"]) > max_entries:
            self.config.parameters["logs"] = self.config.parameters["logs"][-max_entries:]

    def get_logs(self) -> List[str]:
        """获取所有日志"""
        return self.config.parameters.get("logs", [])

    def clear_logs(self) -> None:
        """清空日志"""
        self.config.parameters["logs"] = []

    def set_log_level(self, level: str) -> None:
        """设置日志级别"""
        valid_levels = ["DEBUG", "INFO", "WARNING", "ERROR"]
        if level not in valid_levels:
            raise ValueError(f"Invalid log level: {level}. Must be one of {valid_levels}")
        self.config.parameters["log_level"] = level


class CounterModel(BaseModel):
    """计数器模型"""

    def __init__(self, config: ModelConfig):
        super().__init__(config)
        # 默认参数
        if "count" not in self.config.parameters:
            self.config.parameters["count"] = 0
        if "step" not in self.config.parameters:
            self.config.parameters["step"] = 1
        if "max_count" not in self.config.parameters:
            self.config.parameters["max_count"] = None

    def execute(self, operation: str = "increment") -> int:
        """执行计数操作"""
        if operation == "increment":
            return self.increment()
        elif operation == "decrement":
            return self.decrement()
        elif operation == "reset":
            return self.reset()
        else:
            raise ValueError(f"Unknown operation: {operation}")

    def increment(self) -> int:
        """递增计数"""
        step = self.config.parameters.get("step", 1)
        max_count = self.config.parameters.get("max_count")

        new_count = self.config.parameters["count"] + step

        if max_count is not None and new_count > max_count:
            print(f"[CounterModel] Max count {max_count} reached. Resetting to 0.")
            self.config.parameters["count"] = 0
        else:
            self.config.parameters["count"] = new_count

        return self.config.parameters["count"]

    def decrement(self) -> int:
        """递减计数"""
        step = self.config.parameters.get("step", 1)
        self.config.parameters["count"] = max(0, self.config.parameters["count"] - step)
        return self.config.parameters["count"]

    def reset(self) -> int:
        """重置计数"""
        self.config.parameters["count"] = 0
        return self.config.parameters["count"]

    def get_count(self) -> int:
        """获取当前计数"""
        return self.config.parameters.get("count", 0)

    def set_step(self, step: int) -> None:
        """设置步长"""
        if step <= 0:
            raise ValueError("Step must be positive")
        self.config.parameters["step"] = step

    def set_max_count(self, max_count: Optional[int]) -> None:
        """设置最大计数"""
        if max_count is not None and max_count <= 0:
            raise ValueError("Max count must be positive")
        self.config.parameters["max_count"] = max_count


# ==================== 模型工厂 ====================
class ModelFactory:
    """模型工厂:通过注册字典创建模型"""

    _registry: Dict[str, type] = {"logger": LoggerModel, "counter": CounterModel}

    @classmethod
    def register(cls, model_type: str, model_class: type) -> None:
        """注册新的模型类型"""
        cls._registry[model_type] = model_class

    @classmethod
    def create(cls, model_type: str, config: ModelConfig) -> BaseModel:
        """创建模型实例"""
        if model_type not in cls._registry:
            raise ValueError(f"Unknown model type: {model_type}. Available types: {list(cls._registry.keys())}")

        model_class = cls._registry[model_type]
        return model_class(config)

    @classmethod
    def get_available_types(cls) -> List[str]:
        """获取所有可用的模型类型"""
        return list(cls._registry.keys())


# ==================== 项目管理器 ====================
class ProjectManager:
    """项目管理核心类"""

    def __init__(self):
        self.project_config: Optional[ProjectConfig] = None
        self.models: List[BaseModel] = []

    def open_project(self, project_json_path: Path) -> None:
        """打开现有项目"""
        project_path = project_json_path.parent

        # 加载项目配置
        data = FileManager.load_json(project_json_path)
        self.project_config = ProjectConfig.from_dict(data, project_path, project_json_path)

        # 加载所有模型
        self.models = []
        if project_path.exists():
            for model_dir in project_path.iterdir():
                if model_dir.is_dir():
                    model_config_path = model_dir / f"{model_dir.name}.json"
                    if model_config_path.exists():
                        model_data = FileManager.load_json(model_config_path)
                        model_config = ModelConfig.from_dict(model_data, project_path)
                        model = ModelFactory.create(model_config.model_type, model_config)
                        model.load_config()
                        self.models.append(model)

    def new_project(self, project_path: Path, project_name: str, version: str, models_config: List[Dict[str, Any]]) -> None:
        """创建新项目

        Args:
            project_path: 项目路径
            project_name: 项目名称
            version: 版本号
            models_config: 模型配置列表,格式: [{"type": "logger", "name": "model1", "params": {...}}, ...]
        """
        ValidationRules.validate_version(version)
        FileManager.ensure_dir(project_path)

        project_json_path = project_path / f"{project_name}.json"

        self.project_config = ProjectConfig(project_name=project_name, version=version, project_path=project_path, config_path=project_json_path)

        # 创建模型
        self.models = []
        for model_cfg in models_config:
            model_type = model_cfg.get("type")
            model_name = model_cfg.get("name")
            model_params = model_cfg.get("params", {})

            config = ModelConfig(model_name=model_name, model_type=model_type, description=model_cfg.get("description", ""), parameters=model_params, project_path=project_path)

            model = ModelFactory.create(model_type, config)
            self.models.append(model)

        self.save_project()

    def save_project(self) -> None:
        """保存项目和所有模型"""
        if not self.project_config:
            raise RuntimeError("No project loaded")

        # 保存项目配置
        FileManager.save_json(self.project_config.config_path, self.project_config.to_dict())

        # 保存所有模型
        for model in self.models:
            model.save_config()

    def save_as_project(self, new_project_path: Path) -> None:
        """另存为新项目"""
        if not self.project_config:
            raise RuntimeError("No project loaded")

        FileManager.ensure_dir(new_project_path)

        new_project_json_path = new_project_path / self.project_config.config_path.name

        # 更新项目配置
        self.project_config.project_path = new_project_path
        self.project_config.config_path = new_project_json_path

        # 更新所有模型路径
        for model in self.models:
            model.config.project_path = new_project_path

        self.save_project()

    def create_from_template(self, template_json_path: Path, new_project_path: Path) -> None:
        """从模板创建新项目"""
        FileManager.ensure_dir(new_project_path)

        # 加载模板
        self.open_project(template_json_path)

        # 新主json文件名 = 文件夹名 + .json
        folder_name = new_project_path.name
        new_project_json_path = new_project_path / f"{folder_name}.json"

        # 更新路径
        self.project_config.project_path = new_project_path
        self.project_config.config_path = new_project_json_path

        for model in self.models:
            model.config.project_path = new_project_path

        self.save_project()

        # 重新打开确保一致性
        self.open_project(new_project_json_path)

    def get_model(self, model_name: str) -> Optional[BaseModel]:
        """根据名称获取模型"""
        for model in self.models:
            if model.config.model_name == model_name:
                return model
        return None

    def get_models_by_type(self, model_type: str) -> List[BaseModel]:
        """根据类型获取所有模型"""
        return [m for m in self.models if m.config.model_type == model_type]


# ==================== 应用入口(薄层) ====================
class Application:
    """应用程序入口"""

    def __init__(self, project_json_path: Optional[Path] = None):
        self.manager = ProjectManager()
        if project_json_path:
            self.open_project(project_json_path)

    def open_project(self, project_json_path: Path) -> None:
        """打开项目"""
        if isinstance(project_json_path, str):
            project_json_path = Path(project_json_path)
        self.manager.open_project(project_json_path)

    def new_project(self, project_path: Path, project_name: str, version: str, models_config: List[Dict[str, Any]]) -> None:
        """创建新项目"""
        if isinstance(project_path, str):
            project_path = Path(project_path)
        self.manager.new_project(project_path, project_name, version, models_config)

    def save_project(self) -> None:
        """保存项目"""
        self.manager.save_project()

    def save_as(self, new_project_path: Path) -> None:
        """另存为"""
        if isinstance(new_project_path, str):
            new_project_path = Path(new_project_path)
        self.manager.save_as_project(new_project_path)

    def create_from_template(self, template_path: Path, new_project_path: Path) -> None:
        """从模板创建"""
        if isinstance(template_path, str):
            template_path = Path(template_path)
        if isinstance(new_project_path, str):
            new_project_path = Path(new_project_path)
        self.manager.create_from_template(template_path, new_project_path)

    def get_model(self, model_name: str) -> Optional[BaseModel]:
        """获取模型"""
        return self.manager.get_model(model_name)


# ==================== 单元测试 ====================
if __name__ == "__main__":
    import unittest
    import tempfile
    import shutil

    class TestValidationRules(unittest.TestCase):
        """测试验证规则"""

        def test_valid_version(self):
            """测试有效版本"""
            ValidationRules.validate_version("0.1.0")
            ValidationRules.validate_version("1.0.0")

        def test_invalid_version_format(self):
            """测试无效版本格式"""
            with self.assertRaises(ValueError):
                ValidationRules.validate_version("1.0")
            with self.assertRaises(ValueError):
                ValidationRules.validate_version("v1.0.0")

        def test_version_exceeds_max(self):
            """测试版本超过最大值"""
            with self.assertRaises(ValueError):
                ValidationRules.validate_version("1.0.1")
            with self.assertRaises(ValueError):
                ValidationRules.validate_version("2.0.0")

    class TestFileManager(unittest.TestCase):
        """测试文件管理器"""

        def setUp(self):
            self.test_dir = Path(tempfile.mkdtemp())

        def tearDown(self):
            shutil.rmtree(self.test_dir)

        def test_save_and_load_json(self):
            """测试保存和加载JSON"""
            test_file = self.test_dir / "test.json"
            test_data = {"key": "value", "number": 123}

            FileManager.save_json(test_file, test_data)
            loaded_data = FileManager.load_json(test_file)

            self.assertEqual(test_data, loaded_data)

        def test_load_nonexistent_file(self):
            """测试加载不存在的文件"""
            with self.assertRaises(FileNotFoundError):
                FileManager.load_json(self.test_dir / "nonexistent.json")

    class TestModels(unittest.TestCase):
        """测试模型"""

        def setUp(self):
            self.test_dir = Path(tempfile.mkdtemp())

        def tearDown(self):
            shutil.rmtree(self.test_dir)

        def test_logger_model(self):
            """测试日志模型"""
            config = ModelConfig(model_name="test_logger", model_type="logger", description="Test logger", parameters={}, project_path=self.test_dir)

            logger = LoggerModel(config)
            logger.execute("Test message", "INFO")

            logs = logger.get_logs()
            self.assertEqual(len(logs), 1)
            self.assertIn("Test message", logs[0])

            logger.clear_logs()
            self.assertEqual(len(logger.get_logs()), 0)

        def test_counter_model(self):
            """测试计数器模型"""
            config = ModelConfig(model_name="test_counter", model_type="counter", description="Test counter", parameters={}, project_path=self.test_dir)

            counter = CounterModel(config)

            # 测试递增
            count = counter.increment()
            self.assertEqual(count, 1)

            count = counter.increment()
            self.assertEqual(count, 2)

            # 测试递减
            count = counter.decrement()
            self.assertEqual(count, 1)

            # 测试重置
            count = counter.reset()
            self.assertEqual(count, 0)

        def test_counter_max_count(self):
            """测试计数器最大值"""
            config = ModelConfig(model_name="test_counter", model_type="counter", description="Test counter", parameters={"max_count": 3}, project_path=self.test_dir)

            counter = CounterModel(config)
            counter.increment()  # 1
            counter.increment()  # 2
            counter.increment()  # 3
            count = counter.increment()  # 应该重置为0
            self.assertEqual(count, 0)

    class TestProjectManager(unittest.TestCase):
        """测试项目管理器"""

        def setUp(self):
            self.test_dir = Path(tempfile.mkdtemp())

        def tearDown(self):
            shutil.rmtree(self.test_dir)

        def test_new_project(self):
            """测试创建新项目"""
            project_path = self.test_dir / "test_project"

            app = Application()
            app.new_project(project_path=project_path, project_name="TestProject", version="1.0.0", models_config=[{"type": "logger", "name": "logger1", "params": {}}, {"type": "counter", "name": "counter1", "params": {"step": 2}}])

            # 验证项目文件存在
            self.assertTrue((project_path / "TestProject.json").exists())

            # 验证模型
            logger = app.get_model("logger1")
            self.assertIsNotNone(logger)
            self.assertIsInstance(logger, LoggerModel)

            counter = app.get_model("counter1")
            self.assertIsNotNone(counter)
            self.assertIsInstance(counter, CounterModel)
            self.assertEqual(counter.config.parameters["step"], 2)

        def test_open_and_save_project(self):
            """测试打开和保存项目"""
            project_path = self.test_dir / "test_project"

            # 创建项目
            app = Application()
            app.new_project(project_path=project_path, project_name="TestProject", version="0.9.0", models_config=[{"type": "counter", "name": "counter1", "params": {}}])

            # 修改计数器
            counter = app.get_model("counter1")
            counter.increment()
            counter.increment()
            app.save_project()

            # 重新打开项目
            app2 = Application(project_path / "TestProject.json")
            counter2 = app2.get_model("counter1")
            self.assertEqual(counter2.get_count(), 2)

        def test_save_as_project(self):
            """测试另存为项目"""
            project_path = self.test_dir / "test_project"
            new_path = self.test_dir / "test_project_copy"

            # 创建并修改项目
            app = Application()
            app.new_project(project_path=project_path, project_name="TestProject", version="1.0.0", models_config=[{"type": "counter", "name": "counter1", "params": {}}])

            counter = app.get_model("counter1")
            counter.increment()

            # 另存为
            app.save_as(new_path)

            # 验证新路径存在且数据正确
            self.assertTrue((new_path / "TestProject.json").exists())

            app2 = Application(new_path / "TestProject.json")
            counter2 = app2.get_model("counter1")
            self.assertEqual(counter2.get_count(), 1)

        def test_create_from_template(self):
            """测试从模板创建项目"""
            template_path = self.test_dir / "template_project"
            new_path = self.test_dir / "new_from_template"

            # 创建模板项目
            app = Application()
            app.new_project(project_path=template_path, project_name="TemplateProject", version="1.0.0", models_config=[{"type": "logger", "name": "logger1", "params": {"log_level": "DEBUG"}}, {"type": "counter", "name": "counter1", "params": {"step": 5}}])

            # 从模板创建新项目
            app2 = Application()
            app2.create_from_template(template_path / "TemplateProject.json", new_path)

            # 验证新项目
            self.assertTrue((new_path / "new_from_template.json").exists())

            logger = app2.get_model("logger1")
            self.assertEqual(logger.config.parameters["log_level"], "DEBUG")

            counter = app2.get_model("counter1")
            self.assertEqual(counter.config.parameters["step"], 5)

        def test_invalid_version(self):
            """测试无效版本"""
            project_path = self.test_dir / "test_project"

            app = Application()
            with self.assertRaises(ValueError):
                app.new_project(project_path=project_path, project_name="TestProject", version="1.0.1", models_config=[])  # 超过最大版本

    class TestIntegration(unittest.TestCase):
        """集成测试"""

        def setUp(self):
            self.test_dir = Path(tempfile.mkdtemp())

        def tearDown(self):
            shutil.rmtree(self.test_dir)

        def test_full_workflow(self):
            """测试完整工作流"""
            project_path = self.test_dir / "full_test"

            # 1. 创建项目
            app = Application()
            app.new_project(project_path=project_path, project_name="FullTest", version="0.5.0", models_config=[{"type": "logger", "name": "app_logger", "params": {}}, {"type": "counter", "name": "visit_counter", "params": {"max_count": 10}}])

            # 2. 使用模型
            logger = app.get_model("app_logger")
            counter = app.get_model("visit_counter")

            logger.execute("Application started")
            for i in range(5):
                counter.increment()
                logger.execute(f"Visit count: {counter.get_count()}")

            # 3. 保存
            app.save_project()

            # 4. 重新打开验证
            app2 = Application(project_path / "FullTest.json")
            logger2 = app2.get_model("app_logger")
            counter2 = app2.get_model("visit_counter")

            self.assertEqual(counter2.get_count(), 5)
            self.assertEqual(len(logger2.get_logs()), 6)  # 1个启动日志 + 5个计数日志

            # 5. 另存为
            copy_path = self.test_dir / "full_test_copy"
            app2.save_as(copy_path)

            # 6. 验证副本
            app3 = Application(copy_path / "FullTest.json")
            counter3 = app3.get_model("visit_counter")
            self.assertEqual(counter3.get_count(), 5)

    # 运行单元测试
    print("=" * 70)
    print("开始运行单元测试...")
    print("=" * 70)
    unittest.main(argv=[""], verbosity=2, exit=False)

    print("\n" + "=" * 70)
    print("单元测试完成！开始运行示例演示...")
    print("=" * 70 + "\n")

    # ==================== 使用示例 ====================
    # 创建临时目录用于演示
    demo_dir = Path(tempfile.mkdtemp())
    print(f"演示目录: {demo_dir}\n")

    try:
        # 示例1: 创建新项目
        print("=" * 50)
        print("示例1: 创建新项目")
        print("=" * 50)

        app1 = Application()
        app1.new_project(project_path=demo_dir / "project1", project_name="MyProject", version="1.0.0", models_config=[{"type": "logger", "name": "main_logger", "description": "主日志记录器", "params": {"log_level": "INFO", "max_entries": 100}}, {"type": "counter", "name": "operation_counter", "description": "操作计数器", "params": {"step": 1, "max_count": 100}}])

        logger1 = app1.get_model("main_logger")
        counter1 = app1.get_model("operation_counter")

        logger1.execute("项目已创建", "INFO")
        counter1.increment()
        counter1.increment()

        logger1.execute(f"当前操作次数: {counter1.get_count()}", "INFO")
        app1.save_project()

        print("✓ 项目创建成功")
        print(f"✓ 日志记录数: {len(logger1.get_logs())}")
        print(f"✓ 操作计数: {counter1.get_count()}\n")

        # 示例2: 打开现有项目
        print("=" * 50)
        print("示例2: 打开现有项目")
        print("=" * 50)

        app2 = Application(demo_dir / "project1" / "MyProject.json")
        logger2 = app2.get_model("main_logger")
        counter2 = app2.get_model("operation_counter")

        print(f"✓ 项目名称: {app2.manager.project_config.project_name}")
        print(f"✓ 版本: {app2.manager.project_config.version}")
        print(f"✓ 当前操作计数: {counter2.get_count()}")

        # 继续操作
        for i in range(3):
            counter2.increment()
            logger2.execute(f"执行操作 #{counter2.get_count()}", "INFO")

        app2.save_project()
        print(f"✓ 新的操作计数: {counter2.get_count()}\n")

        # 示例3: 另存为新项目
        print("=" * 50)
        print("示例3: 另存为新项目")
        print("=" * 50)

        app2.save_as(demo_dir / "project1_backup")
        print("✓ 项目已备份到: project1_backup")

        # 验证备份
        app3 = Application(demo_dir / "project1_backup" / "MyProject.json")
        counter3 = app3.get_model("operation_counter")
        print(f"✓ 备份项目操作计数: {counter3.get_count()}\n")

        # 示例4: 从模板创建新项目
        print("=" * 50)
        print("示例4: 从模板创建新项目")
        print("=" * 50)

        app4 = Application()
        app4.create_from_template(template_path=demo_dir / "project1" / "MyProject.json", new_project_path=demo_dir / "project_from_template")

        logger4 = app4.get_model("main_logger")
        counter4 = app4.get_model("operation_counter")

        logger4.execute("从模板创建的新项目", "INFO")
        counter4.reset()
        counter4.increment()

        app4.save_project()
        print(f"✓ 模板项目创建成功")
        print(f"✓ 新项目计数: {counter4.get_count()}\n")

        # 示例5: 测试计数器的最大值功能
        print("=" * 50)
        print("示例5: 测试计数器最大值功能")
        print("=" * 50)

        app5 = Application()
        app5.new_project(project_path=demo_dir / "counter_test", project_name="CounterTest", version="0.8.0", models_config=[{"type": "counter", "name": "limited_counter", "params": {"step": 1, "max_count": 5}}])

        counter5 = app5.get_model("limited_counter")
        print("设置最大计数为5，步长为1")

        for i in range(7):
            count = counter5.increment()
            print(f"  第{i+1}次递增: 计数 = {count}")

        print()

        # 示例6: 测试日志级别
        print("=" * 50)
        print("示例6: 测试不同日志级别")
        print("=" * 50)

        app6 = Application()
        app6.new_project(project_path=demo_dir / "logger_test", project_name="LoggerTest", version="1.0.0", models_config=[{"type": "logger", "name": "multi_level_logger", "params": {"log_level": "DEBUG"}}])

        logger6 = app6.get_model("multi_level_logger")
        logger6.execute("调试信息", "DEBUG")
        logger6.execute("普通信息", "INFO")
        logger6.execute("警告信息", "WARNING")
        logger6.execute("错误信息", "ERROR")

        print(f"✓ 总共记录了 {len(logger6.get_logs())} 条日志")
        print("✓ 日志内容:")
        for log in logger6.get_logs():
            print(f"  {log}")

        print()

        # 示例7: 测试版本验证
        print("=" * 50)
        print("示例7: 测试版本验证（应该失败）")
        print("=" * 50)

        try:
            app7 = Application()
            app7.new_project(project_path=demo_dir / "invalid_version", project_name="InvalidVersion", version="1.0.1", models_config=[])  # 超过最大版本1.0.0
            print("✗ 错误: 应该抛出异常但没有")
        except ValueError as e:
            print(f"✓ 正确捕获版本错误: {e}")

        print()

        # 示例8: 查看可用的模型类型
        print("=" * 50)
        print("示例8: 可用的模型类型")
        print("=" * 50)

        available_types = ModelFactory.get_available_types()
        print(f"✓ 当前支持的模型类型: {', '.join(available_types)}")

        print()
        print("=" * 70)
        print("所有示例演示完成！")
        print("=" * 70)

    finally:
        # 清理演示目录
        print(f"\n清理演示目录: {demo_dir}")
        shutil.rmtree(demo_dir)
