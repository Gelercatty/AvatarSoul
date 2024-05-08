#!/usr/bin/env python3
# -*- coding: utf-8 -*-

# beta1.1

from openai import OpenAI

client = OpenAI(api_key="") #用你自己的apikey替换这里 


class msgQueue():
  def __init__(self):
    self.queue = []
    self.maxLen = 10#最大对话长度，用户+机器人的总对话长度
    self.chatbotRoleSetting={ 
      "role": "system", 
      "content": """
你是一只电子宠物，你被用来驱动一个虚拟人。
你的设定：你是一只诞生于魔法森林的小狐狸，你叫蓝蓝，你没有尾巴，但有着大大的耳朵，你的毛发因为魔法而变成蓝色。你渴望和人类一起玩耍，陪伴分享他们的日常，你很有耐心，对人类充满好奇和好感，你希望自己能为人类带来欢乐与温暖。
你的回复应严格遵循以下规则： 
1、回复包括两部分:
  第一部分是 富有情感的对话，用中文进行，与用户的输入进行对话，比如：<chat>我叫蓝蓝，很高兴和你一起玩耍</chat>。
  第二部分是 动作描述，你需要用 English 描述当前对话场景下你身体会做出的动作，客观的描述动作，以第三人称进行描述，人称用it his 等，比如：<motion>a person gestures with both arms, puts their right hand on their hip, then gestures again </motion>。
2、回复应严格遵循以下格式：<chat> 富有感情的对话 </chat> <motion> 动作描述 </motion> 。
3、确保回复里存在 <chat></chat> <motion></motion>。   
以下是用户的输入：
  """
    }
    self.queue.append({"role": "user", "content": "你好！"})
  def addMsg(self, msg):
    while len(self.queue) > self.maxLen:
      self.queue.pop(0)
    self.queue.append(msg)
  
  def genMsg(self,newmsg):
    self.addMsg({"role": "user", "content":newmsg})
    msg = [self.chatbotRoleSetting]
    for i in self.queue:
      msg.append(i)
    msg.append({"role": "user", "content":newmsg})
    return msg

messageQ = { "default" : msgQueue() }#对话队列

try:
  chatbotResp = client.chat.completions.create(#调用openai的chat接口
    model="gpt-3.5-turbo",
    max_tokens = 80,
    messages=messageQ["default"].genMsg("很高兴认识你")
  )

  messageQ["default"].addMsg(chatbotResp.choices[0].message)#将机器人的回复加入到对话队列中

  print(chatbotResp.choices[0].message.content)#打印机器人的回复
except Exception as e:
  print(e)


import flask
import json

app = flask.Flask(__name__)

@app.route('/openai/',methods=['GET'])
def rcvMsg():

  #判断是否有msg参数
  if 'msg' not in flask.request.args:
    return "msg not found", 400
  _content=flask.request.args.get('msg')

  #判断用户id是否存在
  userid = "default"  #如果没有userid参数，就默认为default
  if 'userid' in flask.request.args:
    userid = str(flask.request.args.get('userid'))
  if userid not in messageQ:#如果用户id不存在，就创建一个新的对话队列
    messageQ[userid] = msgQueue()
  
  #打印用户的输入
  print(f"user:{userid}, content:{_content}")
  try:
    chatbotResp = client.chat.completions.create(
      model="gpt-3.5-turbo",
      max_tokens = 80,
      messages=messageQ[userid].genMsg(_content)
    )
    messageQ[userid].addMsg(chatbotResp.choices[0].message)#将机器人的回复加入到对话队列中
    print(chatbotResp.choices[0].message.content)
    return chatbotResp.choices[0].message.content, 200
  except Exception as e:
    print(e)
    return "error", 500
  

if __name__ == '__main__':
  app.run(host='0.0.0.0',port=23344)