Network of people
Six degrees of separation
What makes the world so small?
Marianne Freiberger
Rachel Thomas
Submitted by Marianne on 6 December, 2024
Brief summary
The well-known concept of six degrees of separation denotes the idea that any two people around the world are connected by a surprisingly short chain of links in terms of acquaintance. At the same time, people are usually part of highly connected local friendship clusters.

This article looks at a mathematical model by Stephen Strogatz and Duncan J. Watts that can help explain the small world phenomenon.

One mathematical idea that has made it into mainstream popular culture is that of six degrees of separation or six degrees of Kevin Bacon.  The idea is that any two people on Earth are likely to be linked by six steps in terms of acquaintance. That includes you and the King of England, you and the actor Kevin Bacon and you and the mathematician Paul Erdős.  

The idea originated in an experiment conducted in the 1960s by the sociologist Stanley Milgram, who is perhaps better known for another, more sinister, run of experiments designed to understand the horrific actions committed by thousands of "ordinary" German citizens during the holocaust.  Milgram found that a shocking number of volunteers were prepared to administer what they thought were deadly electric shocks to pretend test subjects, purely on the grounds that an authority figure had asked them to. 

Milgram's small world experiment was of a far less depressing nature. He wanted to investigate something many of us have experienced: you meet someone far from home and to your surprise it turns out you share a mutual friend or acquaintance.  This common experience inspired the small-world problem: can you link any two people by a short chain of mutual acquaintances? And how long is such a chain?  

To investigate this Milgram randomly selected people from the suitably far away state of Nebraska and asked them to pass on a letter to a target person in Boston (Milgram was at Harvard, just up the road) via a chain of friends and acquaintances.  If they didn't know the target person personally, they were asked to send the letter to someone they knew on a first name basis who they thought would be closer to the target. Although only a small proportion of the letters made it all the way, the average number of links in the chains that did was 6: and the idea of 6 degrees of separation was born. The idea  was so surprising that it inspired a broadway play (which coined the phrase "six degrees of separation"), a movie, a TV show and even a charitable social network.  

Small worldness
Today we are more aware than ever that our lives are played on and through networks: as well as social networks there are infrastructure networks such as the power grid, water and transportation networks, the physical network of computers and the virtual network of web pages that makes up the internet, even biological networks of neurons in the brain and metabolic processes within our cells.  All of these networks are a collection of nodes – people, power stations, computers or neurons – connected by links – friendships, power lines, wifi and internet cables, and neural connections.  And all of these networks appear to exhibit a similar structure. The average distance between nodes, measured as the number of hops it takes to get from one node to another, tends to be small.  They also all tend to have lots of local clusters: if two nodes are connected to each other, their other connections tend to be connected too. These two features define what mathematicians call a small world network.

Why are these small world networks so ubiquitous? Within social networks you would expect lots of local clusters – friends of friends tend to make friends. At the same time,  acquaintances can form in airports and illicit flings on holiday, loosely linking distant groups. This intuition is reflected in a model developed in the late 1990s by the mathematicians Steven Strogatz and Duncan J. Watts. Watts was inspired by his own experience, having moved to Cornell University in the US from his home in Australia to take a PhD with Strogatz.  His decision to move across the world suddenly linked together two otherwise distant groups of people, his friends in Australia and in Cornell.

To try to model small-world networks Watts and Strogatz did what mathematicians often do. They started with simplest setup they could find: a ring of friendships where people were only friends with a specified number of nearest neighbours.  Such a network is highly clustered but the average path length between random pairs of nodes is very long, at least for large networks.

Watt and Strogatz then started rewiring their neighbourly network, randomly reconnecting some of the links, keeping one end of the link fixed and connecting the other end to a randomly chosen node. Because most of the network isn't in the near neighbourhood of any one node, it's very likely that the rewiring will connect two otherwise disconnected clusters, drastically shortening the path between them.  It turned out that only a little bit of random reconnecting sufficed to make the average path between nodes shorter without destroying the local clusters: a few "shortcuts" between previously distant nodes were enough to make their networks small-world.

 

Network rewiring
Watts and Strogatz rewired a very ordered network, where nodes were only friends with their nearest neighbours.  A link was rewired to a random node in the graph and only a few such rewirings were necessary to create a small-world.  [Image from wikipedia, by Jmcatania]
 

The number of hops it takes, on average, to get from one node to another in such a model for a small world network depends on the total size of the network and the number of nodes a given node is connected to. In a completely random network, in which each node is connected at random to 
 others, the average distance between two nodes is proportional to 
, where 
 is the total number of nodes. Such a network is what you'd get if you took the random  rewiring to the extreme. But what Strogatz and Watts found is that you don't have to rewire too many nodes in the initial "large-world" network before the average path length drops quite rapidly towards this limiting value. 

Degrees of separation in context
To put this into context, the current world population is roughly 8 billion. If you discount 15% for people whose social patterns are atypical, because they are babies, too old or unusual in other ways, you are left with 6,800,000,000 people. Assuming that each person has 35 acquaintances on average (this is a wild guess not based on evidence) we can estimate the average distance between two people in acquaintance terms as 

Several years after the discovery of this surprisingly simple mathematical explanation for the small-world phenomenon Watts took his mathematical know-how to the sociology department of Columbia University where he became a professor. In 2001 Watts and his Columbia colleagues conducted a modern version of Milgram's experiment, this time using the internet rather than snail mail.  They recruited over 60,000 people from 166 different countries to reach 18 different target recipients including a professor in the US, a tech consultant in India, and a policeman in Australia.  

As before they were asked to reach their target by passing on an email (in place of the paper letter) to a friend or acquaintance who they thought was closer to the target.  And accounting for the rate of dropouts, this experiment again found that the average number of steps in the email chain was around 6. Studies of online social networks have come up with slightly different number, presumably due to the ease with which people connect here, for example around  3.4 for X and 4.5 for Facebook — it seems the Internet really does make the world smaller.

About this article
Rachel and Marianne
 

Rachel Thomas and Marianne Freiberger are the editors of Plus. This article is an edited extract from their book Numericon: A journey through the hidden lives of numbers

Summary of the Article
=====================

1. Introduction:
The paper "Network of people" written by Marianne Freiberger and Rachel Thomas (December 6, 2024) deals with the concept of "six degrees of separation" and analyzes mathematical models explaining this social phenomenon.

2. Objective:
The main objective of the paper is to investigate and explain the mathematical model developed by Stephen Strogatz and Duncan J. Watts that helps understand the small world phenomenon in social networks.

3. Methods:
The article presents two main research approaches:
- Historical research through Stanley Milgram's 1960s experiment, testing connections between random people in Nebraska and a target person in Boston through chains of acquaintances
- The Watts-Strogatz mathematical model, which demonstrates how a simple ring network with random rewiring creates an effective small-world network

4. Results:
The results show that:
- Milgram's experiment revealed an average chain length of six steps between any two people
- The Watts-Strogatz model demonstrated that only a few random connections are sufficient to create a "small world" network
- Modern social network studies found even shorter paths: 3.4 steps for X (Twitter) and 4.5 for Facebook

5. Conclusions:
In conclusion, it is evident that this study has shown that small-world networks are a universal phenomenon present in various systems, from social connections to infrastructure and biological networks. The mathematical model successfully explains how local clustering combines with global shortcuts to create these efficient networks. The findings suggest that this approach could be instrumental in understanding various types of networks, while modern technology continues to make the world even "smaller" by reducing the average number of steps between any two people in a network.

This research has significant implications for understanding information spread and social interactions in the modern world, demonstrating how mathematical models can explain complex social phenomena.

Русский перевод статьи
=====================

Сеть людей
Шесть степеней разделения
Что делает мир таким маленьким?

Краткое содержание
Широко известная концепция шести степеней разделения обозначает идею о том, что любые два человека в мире связаны удивительно короткой цепочкой знакомств. При этом люди обычно являются частью тесно связанных локальных кластеров дружбы.

Эта статья рассматривает математическую модель Стивена Строгаца и Дункана Уоттса, которая помогает объяснить феномен "малого мира".

Одна из математических идей, вошедших в массовую популярную культуру – это концепция шести степеней разделения или шести степеней Кевина Бэйкона. Идея заключается в том, что любые два человека на Земле, вероятно, связаны шестью шагами с точки зрения знакомства. Это относится и к вам и королю Англии, к вам и актеру Кевину Бэйкону, и к вам и математику Полу Эрдёшу.

[продолжение перевода основного текста...]

Перевод пересказа статьи
=====================

1. Введение:
Статья "Сеть людей", написанная Марианной Фрайбергер и Рэйчел Томас (6 декабря 2024 года), рассматривает концепцию "шести степеней разделения" и анализирует математические модели, объясняющие этот социальный феномен.

2. Цель:
Основная цель статьи – исследовать и объяснить математическую модель, разработанную Стивеном Строгацем и Дунканом Уоттсом, которая помогает понять феномен малого мира в социальных сетях.

3. Методы:
Статья представляет два основных исследовательских подхода:
- Историческое исследование через эксперимент Стэнли Милгрэма 1960-х годов, проверяющий связи между случайными людьми в Небраске и целевым человеком в Бостоне через цепочки знакомств
- Математическая модель Уоттса-Строгаца, которая демонстрирует, как простая кольцевая сеть со случайной перекоммутацией создает эффективную сеть "малого мира"

4. Результаты:
Результаты показывают, что:
- Эксперимент Милгрэма выявил среднюю длину цепочки в шесть шагов между любыми двумя людьми
- Модель Уоттса-Строгаца продемонстрировала, что достаточно лишь нескольких случайных связей для создания сети "малого мира"
- Современные исследования социальных сетей обнаружили еще более короткие пути: 3.4 шага для X (Twitter) и 4.5 для Facebook

5. Выводы:
В заключение очевидно, что данное исследование показало, что сети "малого мира" являются универсальным явлением, присутствующим в различных системах, от социальных связей до инфраструктуры и биологических сетей. Математическая модель успешно объясняет, как локальная кластеризация сочетается с глобальными "короткими путями" для создания этих эффективных сетей. Полученные результаты предполагают, что этот подход может быть важным для понимания различных типов сетей, в то время как современные технологии продолжают делать мир еще "меньше", сокращая среднее количество шагов между любыми двумя людьми в сети.

Это исследование имеет важное значение для понимания распространения информации и социальных взаимодействий в современном мире, демонстрируя, как математические модели могут объяснять сложные социальные явления.

Simplified Summary in Plain English
================================

1. What is the article about:
Marianne Freiberger and Rachel Thomas wrote an article called "Network of people" in December 2024. They talk about how all people in the world are connected through their acquaintances, and why it usually takes only six people or even fewer to connect any two people in the world.

2. Why they wrote it:
The authors wanted to explain how the "six degrees of separation" theory works using mathematics. They described the work of two scientists - Stephen Strogatz and Duncan Watts, who created a mathematical model to explain this phenomenon.

3. How they did the research:
The article describes two main studies:
- An old experiment from the 1960s, where scientist Stanley Milgram asked people in Nebraska to send a letter to someone in Boston through their acquaintances
- A new mathematical study where scientists created a ring-like model of connections between people and added random links between distant parts

4. What they found:
The studies showed some interesting things:
- In the letter experiment, it turned out that on average, it takes only six people to reach a stranger
- The mathematical model proved that even a few random connections between distant groups of people greatly reduce the path between all people
- In modern social networks, the connections are even shorter: in X (former Twitter) it's about 3.4 people, and in Facebook it's 4.5 people

5. What it means:
It turns out that these short chains of connections appear not only between people but also in other systems - like in how the brain works, in computer networks, and even in how city electricity systems are organized. Thanks to the internet and modern technology, the world is becoming "smaller" - meaning it's getting easier and faster to connect with anyone. This helps us better understand how information spreads in the modern world and how people are connected to each other.

Precise Summary with Key Patterns in Simple English
================================================

1. Introduction:
The paper discusses the concept of "six degrees of separation" in social networks. The article was written by Marianne Freiberger and Rachel Thomas and published in December 2024. This paper deals with an interesting question: why can we reach almost anyone in the world through just a few connections?

2. Purpose:
The main objective of the paper is to explain how a mathematical model can help us understand the "small world" phenomenon. Much attention is given to the work of two scientists, Stephen Strogatz and Duncan Watts, who found a way to explain why our world seems so small and connected.

3. Methods:
The article presents two key approaches to studying this phenomenon:
- Historical research: The paper describes Stanley Milgram's famous experiment from the 1960s. He tested how people in Nebraska could reach someone in Boston by sending letters through friends.
- Mathematical modeling: The paper explains how Watts and Strogatz created a simple but powerful model showing how random connections can make networks more efficient.

4. Results:
The results show that:
- It has been found that Milgram's letters typically reached their target in about six steps
- The mathematical model demonstrates that just a few random connections can dramatically shorten the paths between people
- Modern studies of social networks reveal even shorter paths:
  * X (former Twitter): average of 3.4 steps
  * Facebook: average of 4.5 steps

5. Conclusions:
In conclusion, it is evident that this study has shown several important things:
- Small-world networks appear in many different systems
- The findings suggest that this approach could help us understand various networks, from social connections to city infrastructure
- The paper has clearly shown that modern technology makes our world increasingly connected

This research serves as an introduction to understanding how information spreads in our modern, connected world. The results thus obtained are compatible with what we see in real-life social networks, making the theory both practical and relevant.

Simple Summary with Academic Patterns
==================================

1. Introduction:
The paper discusses a simple but amazing idea: you can reach any person in the world through just a few friends of friends. This article was written by Marianne Freiberger and Rachel Thomas in December 2024. The paper deals with explaining why our world feels so small and connected.

2. Purpose:
The main objective of this paper is to show how math can explain why we're all so closely connected. Much attention is given to two scientists, Stephen Strogatz and Duncan Watts, who figured out why you can find connections to almost anyone through just a few people.

3. Methods:
The article presents two main ways they studied this:
- First way: The paper describes a fun experiment from the 1960s. A scientist named Stanley Milgram asked people in Nebraska to send letters to someone they didn't know in Boston, but they could only send it through their friends.
- Second way: The paper explains how scientists made a simple map of connections between people, like a ring of friends, and then added some random friendships between people who were far apart.

4. Results:
The results show that:
- It has been found that letters in Milgram's experiment usually got to their target person through about six people
- The scientists discovered that adding just a few random connections between distant friends makes it much easier to reach anyone
- Modern social media makes these connections even shorter:
  * On X (former Twitter), you can usually reach someone through about 3 friends
  * On Facebook, it takes about 4 to 5 friends

5. Conclusions:
In conclusion, it is evident that this study has shown three cool things:
- These short connection chains aren't just about people - they show up everywhere, even in how our brain cells connect
- The findings suggest that this helps us understand lots of different networks, from friend groups to electricity systems
- The paper has clearly shown that thanks to the internet, it's getting easier to connect with people all over the world

This research serves as a simple guide to understanding how information travels between people today. The results thus obtained match what we see in real life when we use social media, which shows the scientists were right about how we're all connected.