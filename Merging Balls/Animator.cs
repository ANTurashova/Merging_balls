using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace Merging_Balls
{
    public class Animator
    {
        private Graphics _mainG;
        private int _width, _height;
        private readonly List<Animation> _items = new List<Animation>(); //закрытое поле _items типа Animatable //readonly = только для чтения //items = предметы
        private Thread _t;
        private bool _stop;
        private BufferedGraphics _bg;
        private bool _bgChanged;
        private readonly object _obj = new object(); //Все остальные классы в .NET, даже те, которые мы сами создаем, а также базовые типы, такие как System.Int32, являются неявно производными от класса Object
        public Animator(Graphics g, Rectangle r)
        {
            Update(g, r);
        }

        public void Update(Graphics g, Rectangle r)
        {
            _mainG = g;
            _width = r.Width; 
            _height = r.Height;
            Monitor.Enter(_obj);
            _bgChanged = true; //try { _bg.Render(); }
            _bg = BufferedGraphicsManager.Current.Allocate(  //Класс BufferedGraphicsManager позволяет реализовать пользовательскую двойную буферизацию для графики
                _mainG,
                new Rectangle(0, 0, _width, _height)
            );
            Monitor.Exit(_obj);
            Monitor.Enter(_items);
            foreach (var item in _items)
            {
                item.Update(new Rectangle(0, 0, _width, _height));
            }
            Monitor.Exit(_items);
        }

        private void Animate()
        {
            while (!_stop)
            {
                Monitor.Enter(_obj);
                _bgChanged = false;
                Graphics g = _bg.Graphics;
                g.Clear(Color.White);
                Monitor.Exit(_obj);
                Monitor.Enter(_items);
                int cnt = _items.Count;
                for (int i = 0; i < cnt; i++)
                {
                    if (!_items[i].IsAlive)
                    {
                        _items.Remove(_items[i]);
                        i--;
                        cnt--;
                    }
                }
                Monitor.Enter(_obj);
                foreach (var item in _items) { item.Draw(g); }
                Monitor.Exit(_obj);

                Monitor.Exit(_items);
                Monitor.Enter(_obj);
                if (!_bgChanged)
                {
                    try { _bg.Render(); } //BufferedGraphics.Render Метод = Записывает содержимое графического буфера в устройство по умолчанию
                    catch (Exception) //исключение
                    {// ignored
                    }
                }
                Monitor.Exit(_obj);
                Thread.Sleep(30);
            }
        }

        public void Start(Animation item) //~старт бегания шарика
        {
            if (_t == null || !_t.IsAlive)
            {
                _stop = false;
                ThreadStart th = Animate;
                _t = new Thread(th);
                _t.Start();
            }
            Monitor.Enter(_items);
            Thread.Sleep(15);
            item.Start();
            _items.Add(item);
            Monitor.Exit(_items);
        }

        public void Stop()
        {
            _stop = true;
            Monitor.Enter(_items);
            foreach (var item in _items) { item.Stop(); }
            _items.Clear();
            Monitor.Exit(_items);
        }
    }

    public abstract class CommonData<T>
    {
        protected static int MaxSize; //protected=доступ к подклассам 
        protected readonly Queue<T>[] Vals;

        public CommonData(int maxSize)
        {
            MaxSize = maxSize;
            Vals = new Queue<T>[3];

            for (int i = 0; i < 3; i++)
            {
                Vals[i] = new Queue<T>();
            }
        }
        public void Add(int index, T value)
        {
            index = Math.Abs(index % 3);
            var q = Vals[index];
            Monitor.Enter(q);
            try
            {
                while (q.Count >= MaxSize) { Monitor.Wait(q); }
                q.Enqueue(value);
                Monitor.PulseAll(q);
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            finally { Monitor.Exit(q); }
        }
        public abstract T[] GetNextData();
    }
}
