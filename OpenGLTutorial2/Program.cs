using System;
using Tao.FreeGlut;
using OpenGL;

namespace OpenGLTutorial2
{
    class Program
    {
        private static int width = 1280, height = 720;
        private static ShaderProgram program;
        private static VBO<Vector3> triangle, square;
        private static VBO<Vector3> triangleColor, squareColor;

        private static VBO<int> triangleElements, squareElements;

        static void Main(string[] args)
        {
            // 生成OpenGL窗口
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(width, height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            // 设置回调方法
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);
            Glut.glutMouseFunc(OnMouse);

            // 编译shader
            program = new ShaderProgram(VertexShader, FragmentShader);
            if (program.ProgramLog.Length > 1)
            {
                Console.WriteLine(program.ProgramLog);
                return;
            }


            // 设置摄像机矩阵和屏幕矩阵
            program.Use();
            program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
            program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, Vector3.Up));

            // 创建三角形数据VBO
            triangle = new VBO<Vector3>(new Vector3[] { new Vector3(0, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) });
            triangleColor = new VBO<Vector3>(new Vector3[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) });
            triangleElements = new VBO<int>(new int[] { 0, 1, 2 }, BufferTarget.ElementArrayBuffer);

            // 创建四边形数据VBO
            square = new VBO<Vector3>(new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) });
            squareColor = new VBO<Vector3>(new Vector3[] { new Vector3(0.5, 0.5, 1), new Vector3(0.5, 0.5, 1), new Vector3(0.5, 0.5, 1), new Vector3(0.5, 0.5, 1) });
            squareElements = new VBO<int>(new int[] { 0, 1, 2, 2, 3, 0 }, BufferTarget.ElementArrayBuffer);
            //进入主循环
            Glut.glutMainLoop();
        }

        private static void OnMouse(int button, int state, int x, int y)
        {
            Console.WriteLine("鼠标按键：{2},按键状态：{3} ,鼠标位置：{0},{1}", x, y, button, state);
        }

        private static void OnClose()
        {
            // dispose of all of the resources that were created
            triangle.Dispose();
            triangleColor.Dispose();
            triangleElements.Dispose();
            square.Dispose();
            squareColor.Dispose();
            squareElements.Dispose();
            program.DisposeChildren = true;
            program.Dispose();
        }

        private static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            // 设置viewport 并且清空屏幕缓存和深度缓存
            Gl.Viewport(0, 0, width, height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // 使用shader
            Gl.UseProgram(program);

            // 移动模型矩阵位置
            program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));

            //开始绑定三角形数据 通常方法
            // bind the vertex attribute arrays for the triangle (the hard way)
            //uint vertexPositionIndex = (uint)Gl.GetAttribLocation(program.ProgramID, "vertexPosition");
            //Gl.EnableVertexAttribArray(vertexPositionIndex);
            //Gl.BindBuffer(triangle);
            //Gl.VertexAttribPointer(vertexPositionIndex, triangle.Size, triangle.PointerType, true, 12, IntPtr.Zero);
            //Gl.BindBuffer(triangleElements);

            //简单方法
            Gl.BindBufferToShaderAttribute(triangle, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(triangleColor, program, "vertexColor");
            Gl.BindBuffer(triangleElements);

            // draw the triangle
            Gl.DrawElements(BeginMode.Triangles, triangleElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            //结束三角形

            // transform the square
            program["model_matrix"].SetValue(Matrix4.CreateTranslation(new Vector3(1.5f, 0, 0)));

            // bind the vertex attribute arrays for the square (the easy way)
            Gl.BindBufferToShaderAttribute(square, program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(squareColor, program, "vertexColor");
            Gl.BindBuffer(squareElements);

            // draw the square
            Gl.DrawElements(BeginMode.Triangles, squareElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();
        }

        public static string VertexShader = @"
#version 130
in vec3 vertexPosition;
in vec3 vertexColor;

out vec3 color;

uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
void main(void)
{
    color=vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130
in vec3 color;
out vec4 fragment;
void main(void)
{
    fragment = vec4(color, 1);
}
";
    }
}
