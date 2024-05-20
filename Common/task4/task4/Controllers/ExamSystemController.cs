using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using task4.Models;

namespace task4.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExamSystemController : ControllerBase
    {
        private ExamSystem<StudentData> ExSys;

        public ExamSystemController(SetType type)
        {
            ExSys = new ExamSystem<StudentData>(type);
        }


        [HttpPost]
        public JsonResult AddData(StudentData newExam)
        {
            ExSys.Add(newExam);
            return new JsonResult(Ok(newExam.studentId)); 
        }

        [HttpDelete]
        public JsonResult RemoveData(StudentData newExam)
        {
            ExSys.Remove(newExam);
            return new JsonResult(Ok(newExam.studentId));
        }

        [HttpGet]
        public JsonResult ContainData(StudentData newExam)
        {
            return new JsonResult(Ok(ExSys.Contains(newExam)));
        }

        [HttpGet("/GetAll")]
        public JsonResult CountData()
        {
            return new JsonResult(Ok(ExSys.Count()));
        }
    }
}
