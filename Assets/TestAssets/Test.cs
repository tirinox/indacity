using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InDaCity;
using System.Linq;

namespace Test
{

    public class Test : MonoBehaviour
    {    
        public GameObject buildingPrefab;
        public GameObject cityLocation;   

        Vector3? PositionOnTerrain(Vector2 positionOnMap)
        {
            var cityPosition = cityLocation.transform.position;

            // positionate me onto the terrain
            RaycastHit hit;
            if (Physics.Raycast(cityPosition + new Vector3(positionOnMap.x, 0, positionOnMap.y), -Vector3.up, out hit))
            {
                return hit.point;
            }
            return null;
        }

        void BuildHouse(Polygon polygon)
        {
            if (!polygon.valid)
                return;

            var position = PositionOnTerrain(polygon.center);

            // intersection exists && position y is not minimal (not river)
            if (position != null && position.Value.y > 0.1)
            {
                // instantiate a new building
                GameObject b = Instantiate(buildingPrefab);
                b.transform.parent = cityLocation.transform;
                var builder = b.GetComponent<BuildingMesh>();

                // build a building with specified parameters
                builder.Build(polygon, polygon.height);
                b.transform.position = position.Value;
            }
        }

        void CreateVoronoiCity()
        {
            var city = new City();
            city.maxSize = 200.0f;
            city.nPoints = 500;

            //city.CalculateOneQuarterForTest();
            city.CalculatePolygons();

            foreach (var polygon in city.results)
            {
                BuildHouse(polygon);
            }
        }

        void ClearCity()
        {
            foreach (Transform child in cityLocation.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        void RebuildCity()
        {
            var start = Time.realtimeSinceStartup;
            ClearCity();
            CreateVoronoiCity();

            var duration = Time.realtimeSinceStartup - start;
            Debug.Log("City build time = " + duration * 1000.0f + " ms");
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.B))
            {
                RebuildCity();
            }
            if(Input.GetKeyUp(KeyCode.T))
            {
                TestFunc();
            }
        }

        void Start()
        {
            TestFunc();

                RebuildCity();
        }

        void TestFunc()
        {
 
        }
    }
}