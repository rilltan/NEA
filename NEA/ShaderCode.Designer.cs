﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NEA {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ShaderCode {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ShaderCode() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NEA.ShaderCode", typeof(ShaderCode).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColour;
        ///
        ///in vec3 fragPos;
        ///
        ///uniform vec3 gridCentre;
        ///uniform float max;
        ///
        ///void main()
        ///{
        ///    float disx = fragPos.x - gridCentre.x;
        ///    float disz = fragPos.z - gridCentre.z;
        ///    float distance = sqrt(disx*disx + disz*disz);
        ///    FragColour = vec4(0.3f, 0.3f, 0.3f, 1-distance/max);
        ///}.
        /// </summary>
        internal static string fragmentGrid {
            get {
                return ResourceManager.GetString("fragmentGrid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColour;
        ///
        ///in vec3 fragPos;
        ///
        ///uniform vec3 colour;
        ///
        ///void main()
        ///{
        ///    FragColour = vec4(colour, 1.0f);
        ///}.
        /// </summary>
        internal static string fragmentPath {
            get {
                return ResourceManager.GetString("fragmentPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///struct Light {
        ///    vec3 pos;
        ///    vec3 colour;
        ///};
        ///uniform Light lights[10];
        ///uniform int numOfLights;
        ///
        ///out vec4 FragColour;
        ///
        ///in vec3 normal;
        ///in vec3 fragPos;
        ///
        ///uniform vec3 objectColour;
        ///
        ///void main()
        ///{
        ///    vec3 norm = normalize(normal);
        ///    vec3 result = vec3(0.0f);
        ///
        ///    for(int i = 0; i &lt; numOfLights; i++)
        ///    {
        ///        vec3 lightDir = normalize(lights[i].pos - fragPos);
        ///        float diffuseAmount = max(dot(norm,lightDir), 0.0f);
        ///        result += diffuseAmount * l [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string fragmentPlanet {
            get {
                return ResourceManager.GetString("fragmentPlanet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColour;
        ///uniform vec3 objectColour;
        ///
        ///void main()
        ///{
        ///    FragColour = vec4(objectColour, 1.0f);
        ///}.
        /// </summary>
        internal static string fragmentStar {
            get {
                return ResourceManager.GetString("fragmentStar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///out vec4 FragColour;
        ///
        ///in vec4 vertexColour;
        ///in vec2 texCoord;
        ///
        ///uniform sampler2D ourTexture;
        ///
        ///void main()
        ///{
        ///  FragColour = texture(ourTexture, texCoord) * vertexColour;
        ///}.
        /// </summary>
        internal static string fragmentUI {
            get {
                return ResourceManager.GetString("fragmentUI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (location = 0) in vec3 aPos;
        ///
        ///out vec3 fragPos;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///void main()
        ///{
        ///    gl_Position = projection * view * model * vec4(aPos.x, aPos.y, aPos.z, 1.0f);
        ///    fragPos = vec3(model * vec4(aPos, 1.0));
        ///}.
        /// </summary>
        internal static string vertexGrid {
            get {
                return ResourceManager.GetString("vertexGrid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (location = 0) in vec3 aPos;
        ///layout (location = 1) in vec3 aNormal;
        ///
        ///out vec3 normal;
        ///out vec3 fragPos;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///void main()
        ///{
        ///    gl_Position = projection * view * model * vec4(aPos.x, aPos.y, aPos.z, 1.0f);
        ///    normal = aNormal;
        ///    fragPos = vec3(model * vec4(aPos, 1.0));
        ///}.
        /// </summary>
        internal static string vertexPlanet {
            get {
                return ResourceManager.GetString("vertexPlanet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (location = 0) in vec3 aPos;
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///void main()
        ///{
        ///    gl_Position = projection * view * model * vec4(aPos.x, aPos.y, aPos.z, 1.0f);
        ///}.
        /// </summary>
        internal static string vertexStar {
            get {
                return ResourceManager.GetString("vertexStar", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #version 330 core
        ///
        ///layout (location = 0) in vec2 aPos;
        ///layout (location = 1) in vec2 aTexCoord;
        ///layout (location = 2) in vec4 aColour;
        ///
        ///out vec4 vertexColour;
        ///out vec2 texCoord;
        ///
        ///void main()
        ///{
        ///  gl_Position = vec4(aPos, 0.0f, 1.0f);
        ///  vertexColour = vec4(aColour);
        ///  texCoord = vec2(aTexCoord);
        ///}.
        /// </summary>
        internal static string vertexUI {
            get {
                return ResourceManager.GetString("vertexUI", resourceCulture);
            }
        }
    }
}
