using System.Text;
using System.IO;
using static System.Console;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

//MainClass<int> main = new MainClass<int>(123123);

Logger logger = new Logger();

Operation<int> co = new Operation<int>(123123);

// State<int> state = 1;
// State<int> state2 = state;
// int i = state;
// WriteLine(state);
// WriteLine(state2);
// WriteLine(i);

S
public class Operation<T>
{
    T input;
    public Operation(T input)
    {
        this.input = input;
    }
}


public class MainClass<T>
{
    List<Compound<T>> compoundList = new List<Compound<T>>();
}
public class State<T> : EventArgs
{
    public enum FlagState
    {
        None = 0,
        Validate = 1,
        InValidate = 2
    }

    T data;
    FlagState flag = FlagState.Validate;
    int validateCount = 0;

    T Data
    {
        get 
        { 
            if(data is not null)
                return data; 
            else
                throw new Exception(nameof(data));
        }
        set
        {
            data = value switch
            {
                FlagState.None              => default(T)!,
                FlagState.Validate          => value,
                FlagState.InValidate        => throw new Exception(nameof(value)),
                _                           => throw new Exception(nameof(value))
            };
        }
    }

    public State(T input) => this.data = input;

    /// 이런식으로 할거면 생성자에서 형변환을 미리 알려줘야 한다.
    /// 근데 generic도 해당이 되나?
    /// 이 코드는 opeator overloading이 먼저 실행되고 그 다음 실행된다.
    /// public static implicit operator T(State<T> value) => value.Data is not null ? (T)Convert.ChangeType(value.Data, typeof(T)) : throw new Exception(nameof(value));
    public State() : this(new State<T>())
    {

    }
    
    /// 아래 코드는 사용자 정의 암시적 변환 연산자다.
    /// T = State<T> 일때 State<T>.Data를 T에 넣는다.
    /// State<T> = T 일때 new State<T>(value)를 생성한다.
    //public static implicit operator T(State<T> value) => value.Data is not null ? (T)Convert.ChangeType(value.Data, typeof(T)) : throw new Exception(nameof(value));
    public static implicit operator State<T>(T value) => new State<T>(value);
    /// (T)Convert.ChangeType(value.Data, typeof(T))를 써야하는 이유
    /// 기본 데이터 형식을 다른 데이터 형식으로 변환 
    /// 5가지 결과중 하나를 생성한다.
    /// 1. 변환이 없는 경우 => 원래 형식의 인스턴스만 반환 
    /// 2. 변환이 실패한 경우 => throw Exception
    /// 3. 변환이 성공한 경우 => 변환된 인스턴스 반환 
    /// 4. overflow
    /// 5. formatException
    /// Constructor에서 T 타입의 param을 넣었는데, 굳이 Convert.ChangeType에서 throw Exception을 할 이유?
    public static implicit operator T(State<T> value) => value.Data is not null ? value.Data : throw new Exception(nameof(value));

    public int GetValidateCount() => validateCount;
    public FlagState GetCurrentFlag() => flag;
}
public class Behaviour
{
    
    public event EventHandler<EventArgs> behaviour;

    public void AddBehaviour(Action<object, EventArgs> e)
    {
        if(e is not null)
        {
            EventHandler<EventArgs> h = e.Invoke!;
            behaviour += h;
        }
        else
        {
            throw new Exception(nameof(e));
        }
    }

    public void CallBehaviour(EventArgs e)
    {
        if(e is not null && behaviour is not null)
        {
            EventHandler<EventArgs> temp = behaviour;
            temp.Invoke(this, e);
        }
    }
}
public class CustomBehaviour
{
    public delegate void CustomEventHandler(object sender, EventArgs e);
    public event CustomEventHandler? customHandler;
    public void AddBehaviour(Action<object, EventArgs> e)
    {
        if(e is not null)
        {
            CustomEventHandler temp = e.Invoke!;
            customHandler += temp;
        }
        else
        {
            throw new Exception(nameof(e));
        }
    }

    public void CallBehaviour(EventArgs e)
    {
        if(e is not null && customHandler is not null)
        {
            CustomEventHandler temp = customHandler;
            temp?.Invoke(this, e);
        }
    }
}
public class Compound<T>
{
    public State<T> state { get; private set;}
    /// static delete
    public static Behaviour behaviour { get; private set;}
    public static CustomBehaviour customBehaviour { get; private set; }

    static Compound()
    {
        behaviour = new Behaviour();
        customBehaviour = new CustomBehaviour();
    }
    public Compound(T input)
    {
        state = new State<T>(input);
    }

    public void OnAddresssFunction(Action<object, EventArgs> e)
    {
        behaviour.AddBehaviour(e);
    }

    public void OnCallBehaviour()
    {
        behaviour.CallBehaviour(state);
    }

    public void OnAddresssCustomFunction(Action<object, EventArgs> e)
    {
        customBehaviour.AddBehaviour(e);
    }

    public void OnCallCustomBehaviour()
    {
        customBehaviour.CallBehaviour(state);
    }
}




/// 파일명, 경로, 함수명, 라인 넘버등 로깅 이벤트 발생시점의 정보가 자세하게 담겨 있어야 한다.
/// 파일명?     따로 저장안하는데
/// 경로?       마찬가지
/// 함수명?     이건 등록해야겠다.
/// 시간?       이것도
/// 라인넘버?   이것도  

/// 근데 어디에다가 등록해서 가지고 있을까?
public class Logger
{
    #region Logger Components
    public struct Component
    {
        public string MethodName = string.Empty;
        public DateTime InvokeTime = DateTime.MinValue;
        public int LineNumber = 0;

        /// Compound의 event 감지가 필요
        // public Compound<T> target;
        // public Component(Compound<T> target) 
        // {
        //     this.target = target;
        // }
        public Component() { }
    };
    List<Component> components = new List<Component>();    
    #endregion
    
#region Configuration
    public readonly string filePath = @"D:\최상위 루트 코드파일\repos\VSCode\Sample.txt" ;
    StreamReader reader;
    StreamWriter writer;
#endregion


    /// Component 추가
    /// Task 사용하여 실행 도중에 추가가 진행될 수 있게끔 Thread를 조절
    public void Add()
    {
        components.Add(Configuration());
    }

    /// Report가 추가된 Component 생성
    public Component Configuration()
    {
        
        return new Component();
    }   

    /// Report 작성
    /// 양식 정할것
    /// Component parameter add
    public StringBuilder Report()
    {
        StringBuilder builder = new StringBuilder();
        
        

        return builder;
        
    }

    /// 가장 중요한 logger file 생성을 안함
    public void Read()
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            try
            {
                string line = String.Empty;
                line = reader.ReadLine()!;
                while (line != null)
                {
                    WriteLine(line);
                    line = reader.ReadLine()!;
                }
                reader.Close();
            }
            catch (Exception e) { WriteLine("Exception: " + e.Message); }
            finally { WriteLine("Executing finally block."); }
        }
    }

    public void Write(StringBuilder builder)
    {
        using(StreamWriter writer = new StreamWriter(filePath, true, Encoding.Unicode))
        {
            WriteLine(builder.ToString());
        }
    }
    private string NowTime() => DateTime.Now.ToString("yyyy/MM/dd/hh/ss");
}