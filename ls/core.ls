;; Binding test
(define box (fn (contents title) (. 'System.Windows.Forms.MessageBox 'Show contents title (. 'System.Windows.Forms.MessageBoxButtons 'OKCancel) (. 'System.Windows.Forms.MessageBoxIcon 'Error))))

;; Helpers
(define file-text (fn (path) (. 'System.IO.File 'ReadAllText path)))
(define read (fn (str) (. 'libls.Reader 'Read str)))
(define eval (fn (str) (. 'libls.Evaluator 'Eval str *env*)))

;; defn/defmacro
(define defn     (macro (name arglist expression) `(define ,name `(fn    ,arglist ,expression))))
(define defmacro (macro (name arglist expression) `(define ,name `(macro ,arglist ,expression))))

;; Math
(define pi (. 'System.Math 'PI))
(define tau (* pi 2))
(defn sin (x) (. 'System.Math 'Sin x))
(defn cos (x) (. 'System.Math 'Cos x))
(defn tan (x) (. 'System.Math 'Tan x))
(defn rad (x) (* pi (/ x 180.0)))
(defn deg (x) (* x (/ 180.0 pi)))

;; Looping
;; example: (dotimes 3 '(print "yo"))
(defn dotimes (i expr)
  (if (= i 0)
    'done
    (begin
      (eval expr)
      (dotimes (- i 1) expr))))

;; Sqrt (with apologies to Abelson, Sussman, et. al.)
(defn sqrt-iter (guess x)
  (if (good-enough? guess x)
      guess
      (sqrt-iter (improve guess x) x)))

(defn improve (guess x)
  (average guess (/ x guess)))

(defn average (x y)
  (/ (+ x y) 2.0))

(defn square (x) (* x x))

(defn abs (x) (if (< x 0) (- 0 x) x))

(defn good-enough? (guess x)
  (< (abs (- (square guess) x)) 0.001))

(defn sqrt (x) (sqrt-iter 1.0 x))

;; Logic
(defn not (b) (if b false true))
(defmacro when   (p e) `(if ,p ,e null))
(defmacro unless (p e) `(if ,p null ,e))

;; Lists
(defn count  (l) (. l 'Count))
(defn cons   (a b) (if (not b) `(,a) (begin (. b 'Insert 0 a) b)))
(defn first  (l) (. l 'get_Item 0))
(defn rest   (l) (. l 'GetRange 1 (- (count l) 1)))
(defn second (l) (first (rest l)))
(defn take   (n l) (. l 'GetRange 0 n))
(defn drop   (n l) (. l 'GetRange n (- (count l) n)))
(defn map    (f l) (if (= (count l) 0) null (cons (f (first l)) (map f (rest l)))))

;; Hashtables
(defn assoc (h k v) (begin (. h 'set_Item k v) h))

;; Let
;; example: (let-one a 1 (+ a 2)) ;==> 3
;; example: (let (a 1 b (+ 1 a)) (+ a b)) ;==> 3
(defmacro let-one (name value expression) `(`(fn `(,name) ,expression) ,value))
(defmacro let     (l e) `(let-one ,(first l) ,(second l) ,(if (= (count l) 2) e `(let ,(drop 2 l) ,e))))
